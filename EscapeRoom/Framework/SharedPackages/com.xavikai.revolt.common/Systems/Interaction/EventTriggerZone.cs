using System;
using System.Collections.Generic;
using EscapeRoomRevolt.Core.Save;
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
    public sealed class EventTriggerZone : MonoBehaviour, ISaveable
    {
        [Header("Activation")]
        [Tooltip("Only colliders with this tag can activate the zone. Leave empty to accept any collider.")]
        [SerializeField] private string _requiredTag = "Player";
        [Tooltip("Fire the enter event only once until ResetZone is called.")]
        [SerializeField] private bool _oneShot = true;

        [Header("Save System")]
        [Tooltip("Unique ID used by saved games. Filled in automatically; must differ from every other zone.")]
        [SerializeField] private string _saveId = "";

        [Header("Events")]
        [SerializeField] private UnityEvent _onEntered = new UnityEvent();
        [SerializeField] private UnityEvent _onExited = new UnityEvent();

        private readonly Dictionary<Transform, int> _occupants = new Dictionary<Transform, int>();
        private bool _hasTriggered;

        public UnityEvent OnEntered => _onEntered;
        public UnityEvent OnExited => _onExited;
        public bool HasTriggered => _hasTriggered;

        private void Awake()
        {
            EnsureTriggerCollider();
            SaveManager.Instance?.Register(this);
        }

        private void OnDestroy() => SaveManager.Instance?.Unregister(this);

        // A one-shot zone remembers it has fired, so loading a game does not fire it again
        // (start a timer that was already stopped, reopen a door, show an object twice...).
        public string SaveId => string.IsNullOrWhiteSpace(_saveId) ? name : _saveId;

        [Serializable]
        private sealed class ZoneSaveData { public bool hasTriggered; }

        public string SaveData() => JsonUtility.ToJson(new ZoneSaveData { hasTriggered = _hasTriggered });

        public void LoadData(string json)
        {
            ZoneSaveData data = JsonUtility.FromJson<ZoneSaveData>(json);
            if (data == null) return;
            _hasTriggered = data.hasTriggered;
            _occupants.Clear();
        }

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

        private void OnValidate()
        {
            EnsureTriggerCollider();
            if (string.IsNullOrWhiteSpace(_saveId) && !UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this)
                && !Application.isPlaying)
            {
                _saveId = "event_zone_" + Guid.NewGuid().ToString("N");
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif
    }
}
