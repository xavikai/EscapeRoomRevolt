using System;
using EscapeRoomRevolt.Core;
using EscapeRoomRevolt.Core.Save;
using EscapeRoomRevolt.Core.Settings;
using UnityEngine;
using UnityEngine.Events;

namespace EscapeRoomRevolt.Systems.Survival
{
    public enum HorrorEventActivation { PlayerEnters, SanityThreshold, Manual }
    [Serializable] public sealed class HorrorEventState { public bool hasTriggered; }

    /// <summary>Persistent, data-driven horror beat. Animation and scene consequences stay in UnityEvents.</summary>
    public sealed class HorrorEventTrigger : MonoBehaviour, ISaveable
    {
        [SerializeField] private HorrorEventDefinition _definition;
        [SerializeField] private HorrorEventActivation _activation = HorrorEventActivation.PlayerEnters;
        [SerializeField] private UnityEvent _onTriggered;

        private bool _hasTriggered;
        private float _lastTriggeredAt = float.NegativeInfinity;

        public string SaveId => _definition != null && !string.IsNullOrWhiteSpace(_definition.PersistentId)
            ? $"HorrorEvent.{_definition.PersistentId}"
            : $"HorrorEvent.{gameObject.scene.path}.{name}";

        private void Start()
        {
            if (!GameFeatures.IsEnabled(OptionalGameFeature.HorrorEvents))
            {
                enabled = false;
                return;
            }
            SaveManager.Instance?.Register(this);
            if (_activation == HorrorEventActivation.SanityThreshold && SanityController.Instance != null)
            {
                SanityController.Instance.Changed += OnSanityChanged;
                OnSanityChanged(SanityController.Instance.Value);
            }
        }

        private void OnDestroy()
        {
            SaveManager.Instance?.Unregister(this);
            if (SanityController.Instance != null) SanityController.Instance.Changed -= OnSanityChanged;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_activation == HorrorEventActivation.PlayerEnters && (other.CompareTag("Player") || other.GetComponentInParent<EscapeRoomRevolt.Player.PC.PlayerMovement>() != null))
                TryTrigger();
        }

        private void OnSanityChanged(float _) => TryTrigger();

        public bool TryTrigger()
        {
            if (!GameFeatures.IsEnabled(OptionalGameFeature.HorrorEvents)) return false;
            if (_definition == null) return false;
            if (_definition.OnlyOnce && _hasTriggered) return false;
            if (Time.unscaledTime - _lastTriggeredAt < _definition.Cooldown) return false;
            if (SanityController.Instance != null && SanityController.Instance.Normalized > _definition.MaximumSanity) return false;
            if (!(TensionDirector.Instance?.RequestPermission(_definition) ?? true)) return false;

            _hasTriggered = true;
            _lastTriggeredAt = Time.unscaledTime;
            SanityController.Instance?.ApplyStress(_definition.StressApplied);
            if (!string.IsNullOrWhiteSpace(_definition.Subtitle)) EventBus.Publish(new RequestShowSubtitle { text = _definition.Subtitle });
            if (_definition.Audio != null && EscapeRoomRevolt.Systems.Audio.AudioManager.Instance != null)
            {
                bool reduceLoudSounds = GameSettingsService.Instance != null && GameSettingsService.Instance.Data.reduceLoudSounds;
                EscapeRoomRevolt.Systems.Audio.AudioManager.Instance.PlaySoundAt(_definition.Audio, transform.position, reduceLoudSounds ? .5f : 1f);
            }
            _onTriggered?.Invoke();
            return true;
        }

        public string SaveData() => JsonUtility.ToJson(new HorrorEventState { hasTriggered = _hasTriggered });
        public void LoadData(string json)
        {
            HorrorEventState state = JsonUtility.FromJson<HorrorEventState>(json);
            if (state != null) _hasTriggered = state.hasTriggered;
        }
    }
}
