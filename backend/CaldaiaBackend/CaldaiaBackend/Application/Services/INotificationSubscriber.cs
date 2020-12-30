using System;

namespace CaldaiaBackend.Application.Services
{
    public interface INotificationSubscriber
    {
        void Subscribe<TEvent>(string channel, Action<TEvent> handler);
    }
}
