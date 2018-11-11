using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Extensibility
{
    public interface IDomainEvent
    {
    }

    public static class DomainEvents
    {
        private static Dictionary<Type, List<Action<IDomainEvent>>> _subscriptions = new Dictionary<Type, List<Action<IDomainEvent>>>();

        public static void Raise<T>(T eventArgs) where T : IDomainEvent
        {
            var exists = _subscriptions.ContainsKey(typeof(T));
            if (!exists)
                _subscriptions.Add(typeof(T), new List<Action<IDomainEvent>>());

            var allActions= _subscriptions[typeof(T)].Cast < Action<IDomainEvent>>();

            foreach (var action in allActions)
            {
                action.Invoke(eventArgs);
            }
        }

        public static void Subscribe<T>(Action<T> domainAction) where T : IDomainEvent
        {
            var exists = _subscriptions.ContainsKey(typeof(T));
            if (!exists)
                _subscriptions.Add(typeof(T),new List<Action<IDomainEvent>>());

            _subscriptions[typeof(T)].Add(arg => { domainAction((T)arg); });
        }
    }
}
