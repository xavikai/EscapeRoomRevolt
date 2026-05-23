using System;
using System.Collections.Generic;
using UnityEngine;

namespace EscapeRoomRevolt.Core
{
    /// <summary>
    /// Type-safe global Event Bus for decoupled communication between systems.
    /// 
    /// Usage:
    ///   // Subscribe
    ///   EventBus.Subscribe<OnDoorOpened>(HandleDoorOpened);
    ///
    ///   // Publish
    ///   EventBus.Publish(new OnDoorOpened { doorId = "MainDoor" });
    ///
    ///   // Unsubscribe (always do this in OnDestroy!)
    ///   EventBus.Unsubscribe<OnDoorOpened>(HandleDoorOpened);
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _subscribers =
            new Dictionary<Type, List<Delegate>>();

        /// <summary>Subscribes a handler to an event type.</summary>
        public static void Subscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (!_subscribers.ContainsKey(type))
                _subscribers[type] = new List<Delegate>();

            _subscribers[type].Add(handler);
        }

        /// <summary>Unsubscribes a handler from an event type.</summary>
        public static void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (_subscribers.TryGetValue(type, out var handlers))
                handlers.Remove(handler);
        }

        /// <summary>Publishes an event to all subscribed handlers.</summary>
        public static void Publish<T>(T eventData) where T : struct
        {
            var type = typeof(T);
            if (!_subscribers.TryGetValue(type, out var handlers))
                return;

            // Iterate on a copy to avoid issues if handlers unsubscribe mid-publish
            var copy = new List<Delegate>(handlers);
            foreach (var handler in copy)
            {
                try
                {
                    ((Action<T>)handler)?.Invoke(eventData);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[EventBus] Error in handler for {type.Name}: {e}");
                }
            }
        }

        /// <summary>Clears all subscribers. Call this when reloading the game.</summary>
        public static void Clear()
        {
            _subscribers.Clear();
        }

        /// <summary>Returns the number of subscribers for a given event type (useful for debugging).</summary>
        public static int GetSubscriberCount<T>() where T : struct
        {
            var type = typeof(T);
            return _subscribers.TryGetValue(type, out var handlers) ? handlers.Count : 0;
        }
    }
}
