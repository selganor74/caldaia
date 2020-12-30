using Infrastructure.Logging;
using Microsoft.AspNet.SignalR.Hubs;

namespace CaldaiaBackend.SelfHosted.Owin.SignalRHubs
{
    [HubName("settings")]
    public class SettingsHub : BaseHub
    {
        public SettingsHub(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
        }
    }
}