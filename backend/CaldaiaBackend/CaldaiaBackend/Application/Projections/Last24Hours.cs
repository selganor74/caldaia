using System;
using System.Threading.Tasks;
using CaldaiaBackend.Application.Events;
using CaldaiaBackend.Application.Projections.DataModels;
using CaldaiaBackend.Infrastructure;
using Infrastructure.DomainEvents;
using Infrastructure.Logging;

namespace CaldaiaBackend.Application.Projections
{
    public class Last24Hours
    {
        private IEventSubscriber _dispatcher;
        private ILogger _log;
        private ITimeSlotBufferLoaderSaver<AccumulatorStatistics> _loader;
        private CircularTimeSlotBuffer<AccumulatorStatistics> _timeBuffer;
        private Task emptyTask => Task.Run(() => {});

    public Last24Hours(
        IEventSubscriber dispatcher, 
        ITimeSlotBufferLoaderSaver<AccumulatorStatistics> loader,
        ILoggerFactory loggerFactory
        )
        {
            _dispatcher = dispatcher;
            _loader = loader;
            _log = loggerFactory?.CreateNewLogger(GetType().Name) ?? new NullLogger();
        }
        public Task ProcessEvent(AccumulatorsReceived evt)
        {
            _log.Trace("Processing event " + nameof(AccumulatorsReceived), evt);
            var stats = _timeBuffer.GetContentAtReference(evt.timestamp) ?? new AccumulatorStatistics();

            stats.inTermoAccumulatoreAccu_On += evt.inTermoAccumulatoreAccu_On;
            stats.inTermoAmbienteAccu_On += evt.inTermoAmbienteAccu_On;
            stats.outPompaCaminoAccu_On += evt.outPompaCaminoAccu_On;
            stats.outCaldaiaAccu_On += evt.outCaldaiaAccu_On;
            stats.outPompaAccu_On += evt.outPompaAccu_On;
            stats.rotexP1Accu_On += evt.rotexP1Accu_On;

            _timeBuffer.UpdateOrCreateContentAtReference(evt.timestamp, stats);

            _loader.Save(_timeBuffer);
            return emptyTask;
        }

        public void Start()
        {
            _log.Info("Starting Projection " + GetType().Name);
            _timeBuffer = _loader.Load() ?? new CircularTimeSlotBuffer<AccumulatorStatistics>(96, TimeSpan.FromMinutes(15), DateTime.Now);

            _dispatcher.Subscribe<AccumulatorsReceived>(ProcessEvent);
        }

        public string GetCurrentStatisticsAsJson()
        {
            return this._timeBuffer.AsJson();
        }
    }

}
