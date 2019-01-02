using System;
using System.Threading.Tasks;
using CaldaiaBackend.Application.Events;
using CaldaiaBackend.Application.Projections.DataModels;
using CaldaiaBackend.Application.Services;
using CaldaiaBackend.Infrastructure;
using Infrastructure.DomainEvents;
using Infrastructure.Logging;

namespace CaldaiaBackend.Application.Projections
{
    public class LastWeekTemperatures : BaseProjection<TemperaturesReceived>, IDisposable
    {
        private readonly ILogger _log;
        private readonly ITimeSlotBufferLoaderSaver<TemperatureStatistics> _loader;
        private CircularTimeSlotBuffer<TemperatureStatistics> _timeBuffer;
        private static Task EmptyTask => Task.Run(() => { });

        public LastWeekTemperatures(
            IEventSubscriber dispatcher,
            ITimeSlotBufferLoaderSaver<TemperatureStatistics> loader,
            ILoggerFactory loggerFactory
        ) : base(dispatcher, loggerFactory)
        {
            _loader = loader;
            _log = loggerFactory?.CreateNewLogger(GetType().Name) ?? new NullLogger();
        }

        public override void Start()
        {
            _timeBuffer = _loader.Load() ?? 
                          new CircularTimeSlotBuffer<TemperatureStatistics>(
                              numberOfSlots:24 * 7, 
                              slotSize: TimeSpan.FromHours(1), 
                              lastSlotEndTime: DateTime.Now);
            _log.Info("Loaded time buffer");

            base.Start();
        }

        protected override Task HandleEvent(TemperaturesReceived evt)
        {
            _log.Trace("Processing event " + nameof(TemperaturesReceived), evt);
            var stats = _timeBuffer.GetContentAtReference(evt.timestamp)
                        ?? new TemperatureStatistics();

            var detail = TemperatureDetails.FromEvent(evt);
            stats.AddDetail(detail);

            _timeBuffer.UpdateOrCreateContentAtReference(evt.timestamp, stats);

            _loader.Save(_timeBuffer);
            return EmptyTask;
        }

        public string GetCurrentStatisticsAsJson(bool includeDetails = false)
        {
            var timeBufferAsJson = this._timeBuffer.AsJson();
            if (includeDetails) return timeBufferAsJson;

            var dataWithoutDetails = CircularTimeSlotBuffer<TemperatureStatisticsWithNoDetails>.FromJson(timeBufferAsJson);

            return dataWithoutDetails.AsJson();
        }

        public void Dispose()
        {
            Stop();
            var disposableLoader = _loader as IDisposable;
            disposableLoader?.Dispose();
        }
    }
}