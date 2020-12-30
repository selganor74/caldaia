using System;
using System.Collections.Generic;
using CaldaiaBackend.Application.Services;
using Infrastructure.Events;

namespace CaldaiaBackend.SelfHosted.Infrastructure.Notification
{
    public class NotificationEvent
    {
        public string channel { get; set; }
        public object payload { get; set; }
        public Type payloadType { get; set; }
    }

    public class InMemoryNotificationService : INotificationPublisher, INotificationSubscriber
    {
        private readonly IEventDispatcher _dispatcher;
        private readonly IEventSubscriber _subscriber;
        private Dictionary<string, List<Action<object>>> _handlers = new Dictionary<string, List<Action<object>>>();
        public InMemoryNotificationService(
            IEventDispatcher dispatcher, 
            IEventSubscriber subscriber
            )
        {
            _dispatcher = dispatcher;
            _subscriber = subscriber;
        }

        public void Subscribe<TData>(string channel, Action<TData> handler)
        {
            var handlersForChannel = GetHandlersForChannel(channel);
            Action<object> handlerToAdd = obj => handler((TData)obj);
            handlersForChannel.Add(handlerToAdd);
        }

        private List<Action<object>> GetHandlersForChannel(string channel)
        {
            if (!_handlers.ContainsKey(channel))
                _handlers[channel] = new List<Action<object>>();

            return _handlers[channel];
        }

        public void Notify<TData>(string channel, TData data)
        {
            var handlersForChannel = GetHandlersForChannel(channel);

            foreach (var handler in handlersForChannel)
                handler(data);
            
        }

    }
}
