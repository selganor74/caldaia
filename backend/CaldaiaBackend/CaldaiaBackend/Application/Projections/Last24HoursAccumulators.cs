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
    public class Last24HoursAccumulators : BaseProjection<AccumulatorsReceived>, IDisposable
    {
        private readonly ILogger _log;
        private readonly ITimeSlotBufferLoaderSaver<AccumulatorStatistics> _loader;
        private CircularTimeSlotBuffer<AccumulatorStatistics> _timeBuffer;
        private static Task EmptyTask => Task.Run(() => {});

    public Last24HoursAccumulators(
        IEventSubscriber dispatcher, 
        ITimeSlotBufferLoaderSaver<AccumulatorStatistics> loader,
        ILoggerFactory loggerFactory
        ) : base(dispatcher, loggerFactory)
        {
            _loader = loader;
            _log = loggerFactory?.CreateNewLogger(GetType().Name) ?? new NullLogger();
        }

        public override void Start()
        {
            _timeBuffer = _loader.Load() ?? new CircularTimeSlotBuffer<AccumulatorStatistics>(96, TimeSpan.FromMinutes(15), DateTime.Now);
            _log.Info("Loaded timeBuffer");

            base.Start();
        }

        protected override void HandleEvent(AccumulatorsReceived evt)
        {
            _log.Trace("Processing event " + nameof(AccumulatorsReceived), evt);
            var stats = _timeBuffer.GetContentAtReference(evt.timestamp) ?? new AccumulatorStatistics();

            stats.AddAccumulatorsReceivedEvent(evt);

            _timeBuffer.UpdateOrCreateContentAtReference(evt.timestamp, stats);

            _loader.Save(_timeBuffer);
        }

        public string GetCurrentStatisticsAsJson()
        {
            return this._timeBuffer.AsJson();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
