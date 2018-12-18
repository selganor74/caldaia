using System.Threading.Tasks;
using Infrastructure.Logging;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace CaldaiaBackend.SelfHosted.Owin.SignalR
{
    [HubName("data")]
    public class DataHub : Hub
    {
        private readonly ILogger _log;

        public DataHub(ILoggerFactory loggerFactory)
        {
            _log = (loggerFactory ?? new NullLoggerFactory()).CreateNewLogger(GetType().Name);
        }

        public override Task OnConnected()
        {
            _log.Info($"Client connected to {nameof(DataHub)}");
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            _log.Info($"Client disconnected to {nameof(DataHub)}", stopCalled);
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            _log.Info($"Client reconnected to {nameof(DataHub)}");
            return base.OnReconnected();
        }
    }
}
