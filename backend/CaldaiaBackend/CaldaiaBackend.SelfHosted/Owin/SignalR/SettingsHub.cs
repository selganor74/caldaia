using System;
using System.Threading.Tasks;
using Infrastructure.Logging;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace CaldaiaBackend.SelfHosted.Owin.SignalR
{
    [HubName("settings")]
    public class SettingsHub : Hub
    {
        private readonly ILogger _log;

        public SettingsHub(ILoggerFactory loggerFactory)
        {
            _log = (loggerFactory ?? new NullLoggerFactory()).CreateNewLogger(GetType().Name);
        }

        public override Task OnConnected()
        {
            _log.Info($"Client connected to {nameof(SettingsHub)}");
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            _log.Info($"Client disconnected to {nameof(SettingsHub)}", stopCalled);
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            _log.Info($"Client reconnected to {nameof(SettingsHub)}");
            return base.OnReconnected();
        }
    }
}