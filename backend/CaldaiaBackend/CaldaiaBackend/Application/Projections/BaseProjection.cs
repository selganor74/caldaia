using System;
using Infrastructure.DomainDesign.DomainEvents;
using Infrastructure.Events;
using Infrastructure.Logging;

namespace CaldaiaBackend.Application.Projections
{
    public abstract class BaseProjection<TEvent> 
        where TEvent : class, IDomainEvent
    {
        private readonly IEventSubscriber _subscriber;
        protected readonly ILogger _log;
        private Action _unsubscribe;

        protected BaseProjection(
            IEventSubscriber subscriber,
            ILoggerFactory loggerFactory
            )
        {
            _subscriber = subscriber;
            _log = loggerFactory?.CreateNewLogger(GetType().Name) ?? new NullLogger();
        }

        ~BaseProjection()
        {
            Stop();
        }

        public virtual void Start()
        {
            _log.Info($"Starting Projection {GetType().Name}<{nameof(TEvent)}>");
            _unsubscribe = _subscriber.SubscribeTo<TEvent>(HandleEvent);
        }

        public void Stop()
        {
            _unsubscribe?.Invoke();
            _log.Info($"Stopped Projection {GetType().Name}<{nameof(TEvent)}>");
        }

        protected abstract void HandleEvent(TEvent arg);
    }
}
