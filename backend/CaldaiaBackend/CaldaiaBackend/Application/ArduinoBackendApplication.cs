using System;
using System.Threading;
using CaldaiaBackend.Application.Commands;
using CaldaiaBackend.Application.Interfaces;
using Infrastructure.Actions.Command.Executor;
using Infrastructure.Actions.Query.Executor;
using Infrastructure.Application;
using Infrastructure.Logging;
using Infrastructure.MiscPatterns.Notification;

namespace CaldaiaBackend.Application
{
    public class ArduinoBackendApplication : BaseApplication
    {
        private readonly IArduinoDataReader _dataReader;
        private readonly INotificationPublisher _publisher;
        private Timer _backgroundJob;
        private readonly int _pollIntervalMilliseconds = 5000;
        private readonly ILogger _log;

        public ArduinoBackendApplication(
            IArduinoDataReader dataReader,
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
        }

        protected override void onAppStarting()
        {
            _dataReader.RegisterObserver(
                data => _publisher.Notify("data", data)
                );

            _dataReader.RegisterSettingsObserver(
                settings => _publisher.Notify("settings", settings)
                );

            _backgroundJob = new Timer(pollData, null, _pollIntervalMilliseconds, _pollIntervalMilliseconds);
        }

        private void pollData(object state)
        {
            try
            {
                var cmd = new ReadDataFromArduinoCommand();
                commandExecutor.Execute(cmd);
            }
            catch (Exception e)
            {
                _log.Error("Errors while polling data from Arduino", e);
            }
        }
    }
}
