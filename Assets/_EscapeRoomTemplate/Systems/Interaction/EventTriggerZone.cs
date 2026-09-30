using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace EscapeRoomRevolt.Systems.Interaction
{
    /// <summary>
    /// Generic passage zone that exposes enter and exit events in the Inspector. Wire these events
    /// to any public action, such as GameOverTimer.StartTimer or MovingHazard.StartHazard.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class EventTriggerZone : MonoBehaviour
    {
        [Header("Activation")]
        [Tooltip("Only colliders with this tag can activate the zone. Leave empty to accept any collider.")]
        [SerializeField] private string _requiredTag = "Player";
        [Tooltip("Fire the enter event only once until ResetZone is called.")]
        [SerializeField] private bool _oneShot = true;

        [Header("Events")]
        [SerializeField] private UnityEvent _onEntered = new UnityEvent();
        [SerializeField] private UnityEvent _onExited = new UnityEvent();

        private readonly Dictionary<Transform, int> _occupants = new Dictionary<Transform, int>();
        private bool _hasTriggered;

        public UnityEvent OnEntered => _onEntered;
        public UnityEvent OnExited => _onExited;
        public bool HasTriggered => _hasTriggered;

        private void Awake() => EnsureTriggerCollider();

        private void OnTriggerEnter(Collider other)
        {
            if (!Matches(other)) return;

            Transform actor = ResolveActor(other);
            _occupants.TryGetValue(actor, out int colliderCount);
            _occupants[actor] = colliderCount + 1;
            if (colliderCount > 0 || (_oneShot && _hasTriggered)) return;

            _hasTriggered = true;
            _onEntered?.Invoke();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!Matches(other)) return;

            Transform actor = ResolveActor(other);
            if (!_occupants.TryGetValue(actor, out int colliderCount)) return;
            if (colliderCount > 1)
            {
                _occupants[actor] = colliderCount - 1;
                return;
            }

            _occupants.Remove(actor);
            _onExited?.Invoke();
        }

        /// <summary>Allows a one-shot zone to be armed again from a UnityEvent.</summary>
        public void ResetZone()
        {
            _hasTriggered = false;
            _occupants.Clear();
        }

        private bool Matches(Collider other)
        {
            return other != null && (string.IsNullOrWhiteSpace(_requiredTag) || other.CompareTag(_requiredTag));
        }

        private static Transform ResolveActor(Collider other)
        {
            return other.attachedRigidbody != null ? other.attachedRigidbody.transform.root : other.transform.root;
        }

        private void EnsureTriggerCollider()
        {
            Collider trigger = GetComponent<Collider>();
            if (trigger != null) trigger.isTrigger = true;
        }

#if UNITY_EDITOR
        private void Reset() => EnsureTriggerCollider();

        private void OnValidate() => EnsureTriggerCollider();
#endif
    }
}
