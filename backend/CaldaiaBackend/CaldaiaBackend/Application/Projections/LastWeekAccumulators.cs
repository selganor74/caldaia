using System;
using System.Threading.Tasks;
using CaldaiaBackend.Application.Events;
using CaldaiaBackend.Application.Projections.DataModels;
using CaldaiaBackend.Application.Services;
using CaldaiaBackend.Infrastructure;
using Infrastructure.Events;
using Infrastructure.Logging;

namespace CaldaiaBackend.Application.Projections
{
    public class LastWeekAccumulators : BaseProjection<AccumulatorsReceived>, IDisposable
    {
        private readonly ITimeSlotBufferLoaderSaver<AccumulatorStatistics> _loader;
        private CircularTimeSlotBuffer<AccumulatorStatistics> _timeBuffer;
        private static Task EmptyTask => Task.Run(() => { });

        public void Dispose()
        {
            Stop();
            var loaderAsDisposable = _loader as IDisposable;
            loaderAsDisposable?.Dispose();
        }

        public LastWeekAccumulators(
            ITimeSlotBufferLoaderSaver<AccumulatorStatistics> loader,
            IEventSubscriber subscriber, 
            ILoggerFactory loggerFactory
            ) : base(subscriber, loggerFactory)
        {
            _loader = loader;
        }

        public override void Start()
        {
            _timeBuffer = _loader.Load() ?? new CircularTimeSlotBuffer<AccumulatorStatistics>(
                              numberOfSlots: 24 * 7, 
                              slotSize: TimeSpan.FromHours(1), 
                              lastSlotEndTime: DateTime.Now);
            base.Start(); 
        }

        public string GetCurrentStatisticsAsJson()
        {
            var timeBufferAsJson = _timeBuffer.AsJson();

            return timeBufferAsJson;
        }

        protected override void HandleEvent(AccumulatorsReceived evt)
        {
            _log.Trace("Processing event " + nameof(AccumulatorsReceived), evt);
            var stats = _timeBuffer.GetContentAtReference(evt.timestamp) ?? new AccumulatorStatistics();

            stats.AddAccumulatorsReceivedEvent(evt);

            _timeBuffer.UpdateOrCreateContentAtReference(evt.timestamp, stats);

            _loader.Save(_timeBuffer);
        }
    }
}
