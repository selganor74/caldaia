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
    public class Last24HoursTemperatures
    {
        private readonly IEventSubscriber _dispatcher;
        private readonly ILogger _log;
        private readonly ITimeSlotBufferLoaderSaver<TemperatureStatistics> _loader;
        private CircularTimeSlotBuffer<TemperatureStatistics> _timeBuffer;
        private static Task EmptyTask => Task.Run(() => { });

        public Last24HoursTemperatures(
            IEventSubscriber dispatcher,
            ITimeSlotBufferLoaderSaver<TemperatureStatistics> loader,
            ILoggerFactory loggerFactory
        )
        {
            _dispatcher = dispatcher;
            _loader = loader;
            _log = loggerFactory?.CreateNewLogger(GetType().Name) ?? new NullLogger();
        }
        public Task ProcessEvent(TemperaturesReceived evt)
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

        public void Start()
        {
            _log.Info("Starting Projection " + GetType().Name);
            _timeBuffer = _loader.Load() ?? new CircularTimeSlotBuffer<TemperatureStatistics>(96, TimeSpan.FromMinutes(15), DateTime.Now);

            _dispatcher.Subscribe<TemperaturesReceived>(ProcessEvent);
        }

        public string GetCurrentStatisticsAsJson()
        {
            return this._timeBuffer.AsJson();
        }
    }
}