using System;
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
        private IArduinoDataReader _dataReader;
        private INotificationPublisher _publisher;

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
        }

        protected override void onAppStarting()
        {
            _dataReader.RegisterObserver(
                data => _publisher.Notify("data", data)
                );

            _dataReader.RegisterSettingsObserver(
                settings => _publisher.Notify("settings", settings)
                );
        }
    }
}
