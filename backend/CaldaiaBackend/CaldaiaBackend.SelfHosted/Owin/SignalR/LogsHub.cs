using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace CaldaiaBackend.SelfHosted.Owin.SignalR
{

    [HubName("logs")]
    public class LogsHub : Hub
    {

    }
}