using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace CaldaiaBackend.SelfHosted.Infrastructure.SignalRLogging
{
    [HubName("logs")]
    public class LogsHub : Hub
    {

    }
}
