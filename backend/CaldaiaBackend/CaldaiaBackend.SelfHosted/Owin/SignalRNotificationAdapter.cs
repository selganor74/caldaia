using System;
using CaldaiaBackend.Application.DataModels;
using CaldaiaBackend.Application.Services;
using Infrastructure.Logging;
using Microsoft.AspNet.SignalR;

namespace CaldaiaBackend.SelfHosted.Owin
{
    public class SignalRNotificationAdapter
    {
        private readonly ILogger _log;
        private readonly INotificationSubscriber _appObservable;

        public SignalRNotificationAdapter(
            INotificationSubscriber appObservable,
            ILoggerFactory loggerFactory
            )
        {
            _log = loggerFactory?.CreateNewLogger(GetType().Name) ?? new NullLogger();
            _appObservable = appObservable;
        }

        public void Start()
        {
            _appObservable.Subscribe("data", (DataFromArduino data) => { NotifyToChannel("data", data); });
            _appObservable.Subscribe("settings", (SettingsFromArduino settings) => { NotifyToChannel("settings", settings); });
            _appObservable.Subscribe("raw", (string rawData) => { NotifyToChannel("raw", rawData); });
        }

        public void NotifyToChannel<T>(string channel, T data)
        {
            try
            {
                var channelNotifier = GlobalHost.ConnectionManager.GetHubContext(channel);
                channelNotifier?.Clients.All.notify(data);
            }
            catch (Exception e)
            {
                _log.Warning("Errors in Hub.", e);
            }
        }
    }
}
