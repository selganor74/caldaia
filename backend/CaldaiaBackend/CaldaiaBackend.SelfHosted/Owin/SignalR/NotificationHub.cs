using System;
using Infrastructure.Logging;
using Microsoft.AspNet.SignalR;

namespace CaldaiaBackend.SelfHosted.Owin.SignalR
{
    public class NotificationHub : Hub
    {
        private static ILogger _log;

        public NotificationHub(ILoggerFactory loggerFactory)
        {
            _log = (loggerFactory ?? new NullLoggerFactory()).CreateNewLogger(GetType().Name);
        }
        public static void NotifyToChannel<T>(string channel, T data)
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
