using System;
using System.Threading;
using CaldaiaBackend.Application.Commands;
using CaldaiaBackend.Application.Projections;
using CaldaiaBackend.Application.Services;
using Infrastructure.Actions.Command.Executor;
using Infrastructure.Actions.Query.Executor;
using Infrastructure.Application;
using Infrastructure.Logging;
using Infrastructure.MiscPatterns.Notification;

namespace CaldaiaBackend.Application
{
    public class ArduinoBackendApplication : BaseApplication, IDisposable
    {
        private Timer _dataPollerJob;

        private readonly IArduinoDataReader _dataReader;
        private readonly INotificationPublisher _publisher;
        private readonly int _pollIntervalMilliseconds = 5000;
        private readonly ILogger _log;
        private readonly Last24HoursAccumulators _last24HoursProjection;
        private readonly Last24HoursTemperatures _last24HoursTempsProjection;
        private readonly LastWeekAccumulators _lastWeekAccumulatorsProjection;
        private readonly LastWeekTemperatures _lastWeekTemperaturesProjection;

        public ArduinoBackendApplication(
            IArduinoDataReader dataReader,
            Last24HoursAccumulators last24HoursProjection,
            Last24HoursTemperatures last24HoursTempsProjection,
            LastWeekAccumulators lastWeekAccumulatorsProjection,
            LastWeekTemperatures lastWeekTemperaturesProjection,

            ICommandExecutor theCommandExecutor,
            IQueryExecutor theQueryExecutor,
            INotificationSubscriber theNotificationSubscriber,
            ICommandExecutorConfig theCommandExecutorConfig,
            IQueryExecutorConfig theQueryExecutorConfig,
            INotificationPublisher theNotificationPublisher,
            ILoggerFactory theLoggerFactory
        ) : base(
            theCommandExecutor,
            theQueryExecutor,
            theNotificationSubscriber,
            theCommandExecutorConfig,
            theQueryExecutorConfig,
            theNotificationPublisher,
            theLoggerFactory
        )
        {
            _dataReader = dataReader;
            _publisher = theNotificationPublisher;
            _log = theLoggerFactory?.CreateNewLogger(GetType().Name) ?? new NullLogger();
            _last24HoursProjection = last24HoursProjection;
            _last24HoursTempsProjection = last24HoursTempsProjection;
            _lastWeekAccumulatorsProjection = lastWeekAccumulatorsProjection;
            _lastWeekTemperaturesProjection = lastWeekTemperaturesProjection;
        }

        protected override void onAppStarting()
        {
            RegisterObservers();

            StartProjections();

            StartDataPollerTask();
        }

        private void StartDataPollerTask()
        {
            _dataPollerJob = new Timer(pollData, null, _pollIntervalMilliseconds, _pollIntervalMilliseconds);
        }

        private void StartProjections()
        {
            _last24HoursProjection.Start();
            _last24HoursTempsProjection.Start();
            _last24HoursTempsProjection.Start();
            _lastWeekTemperaturesProjection.Start();
        }

        private void RegisterObservers()
        {
            _dataReader.RegisterObserver(
                data => _publisher.Notify("data", data)
                );

            _dataReader.RegisterSettingsObserver(
                settings => _publisher.Notify("settings", settings)
                );

            _dataReader.RegisterRawDataObserver(
                rawString => _publisher.Notify("raw", rawString)
                );
        }

        private void pollData(object state)
        {
            try
            {
                var cmd = new ReadDataAndResetAccumulatorsCommand();
                commandExecutor.Execute(cmd);
            }
            catch (Exception e)
            {
                _log.Error("Errors while polling data from Arduino", e);
            }
        }

        public void PausePollerForSeconds(int seconds)
        {
            _dataPollerJob.Change(seconds * 1000, _pollIntervalMilliseconds);
        }

        public void Dispose()
        {
            _dataPollerJob?.Dispose();
            _last24HoursProjection?.Dispose();
            _last24HoursTempsProjection?.Dispose();
            _lastWeekTemperaturesProjection?.Dispose();
            _lastWeekAccumulatorsProjection?.Dispose();
        }
    }
}
