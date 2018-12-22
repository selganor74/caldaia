using Infrastructure.Logging;
using Microsoft.AspNet.SignalR.Hubs;

namespace CaldaiaBackend.SelfHosted.Owin.SignalRHubs
{
    [HubName("data")]
    public class DataHub : BaseHub
    {
        public DataHub(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
        }
    }
}
