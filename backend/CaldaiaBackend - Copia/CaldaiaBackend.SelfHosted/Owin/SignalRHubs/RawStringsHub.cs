using Infrastructure.Logging;
using Microsoft.AspNet.SignalR.Hubs;

namespace CaldaiaBackend.SelfHosted.Owin.SignalRHubs
{
    [HubName("raw")]
    public class RawStringsHub : BaseHub
    {
        public RawStringsHub(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
        }
    }
}
