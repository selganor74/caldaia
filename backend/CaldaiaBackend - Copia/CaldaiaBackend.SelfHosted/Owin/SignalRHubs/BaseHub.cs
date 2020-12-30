using System.Threading.Tasks;
using Infrastructure.Logging;
using Microsoft.AspNet.SignalR;

namespace CaldaiaBackend.SelfHosted.Owin.SignalRHubs
{
    public abstract class BaseHub : Hub
    {
        private readonly ILogger _log;

        public BaseHub(ILoggerFactory loggerFactory)
        {
            _log = (loggerFactory ?? new NullLoggerFactory()).CreateNewLogger(GetType().Name);
        }

        public override Task OnConnected()
        {
            _log.Info($"Client connected to {GetType().Name}");
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            _log.Info($"Client disconnected to {GetType().Name}", stopCalled);
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            _log.Info($"Client reconnected to {GetType().Name}");
            return base.OnReconnected();
        }

    }
}
