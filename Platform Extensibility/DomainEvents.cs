using System;
using System.Collections.Concurrent;

namespace WarpCore.Platform.Extensibility
{
    public interface IDomainEvent
    {
    }

    public class DomainEventSubscriberList : ConcurrentBag<Action<IDomainEvent>>
    {
        public void Add<T>(Action<T> action) where T : IDomainEvent
        {
            base.Add(arg => { action((T) arg); });
        }
    }

    public static class DomainEvents
    {
        private static readonly ConcurrentDictionary<Type, DomainEventSubscriberList> _subscriptions = new ConcurrentDictionary<Type, DomainEventSubscriberList>();

        public static void Raise<T>(T eventArgs) where T : IDomainEvent
        {
            var subscriberList = GetOrCreateDomainEventSubscriberList<T>();

            foreach (var subscriber in subscriberList)
                subscriber.Invoke(eventArgs);
            
        }

        public static void Subscribe<T>(Action<T> domainAction) where T : IDomainEvent
        {
            var list = GetOrCreateDomainEventSubscriberList<T>();
            list.Add(domainAction);
        }

        private static DomainEventSubscriberList GetOrCreateDomainEventSubscriberList<T>()
            where T : IDomainEvent
        {
            var domainEventKey = typeof(T);

            var exists = _subscriptions.ContainsKey(domainEventKey);
            if (!exists)
                _subscriptions.TryAdd(domainEventKey, new DomainEventSubscriberList());

            var list = _subscriptions[domainEventKey];
            return list;
        }
    }
}
