using System;
using EscapeRoomRevolt.Core.Save;
using EscapeRoomRevolt.Core.Settings;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Survival
{
    [Serializable] public class SanityState { public float value; }
    public enum SanityStage { Stable, Uneasy, Distressed, Critical }
    /// <summary>Event-driven sanity resource; encounters call ApplyStress and safe zones call Recover.</summary>
    public sealed class SanityController : MonoBehaviour, ISaveable
    {
        public static SanityController Instance { get; private set; }

        [SerializeField] private SanityProfile _profile;
        [SerializeField, Range(0f, 100f)] private float _sanity = 100f;
        [SerializeField] private float _passiveRecoveryPerSecond = .5f;
        public float Value => _sanity;
        public float Maximum => _profile != null ? _profile.Maximum : 100f;
        public float Normalized => Maximum <= 0f ? 0f : _sanity / Maximum;
        public SanityStage Stage { get; private set; }
        public event Action<float> Changed;
        public event Action<SanityStage> StageChanged;

        private void Awake()
        {
            if (!GameFeatures.IsEnabled(OptionalGameFeature.Sanity))
            {
                enabled = false;
                return;
            }
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            _sanity = Mathf.Clamp(_sanity, 0f, Maximum);
            Stage = EvaluateStage(Normalized);
            SaveManager.Instance?.Register(this);
        }

        private void OnDestroy()
        {
            SaveManager.Instance?.Unregister(this);
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            float recovery = _profile != null ? _profile.PassiveRecoveryPerSecond : _passiveRecoveryPerSecond;
            if (_sanity < Maximum && recovery > 0f) Recover(recovery * Time.deltaTime);
        }
        public void ApplyStress(float amount) { if (GameFeatures.IsEnabled(OptionalGameFeature.Sanity)) Set(_sanity - Mathf.Abs(amount)); }
        public void Recover(float amount) { if (GameFeatures.IsEnabled(OptionalGameFeature.Sanity)) Set(_sanity + Mathf.Abs(amount)); }
        public void SetNormalized(float value) { if (GameFeatures.IsEnabled(OptionalGameFeature.Sanity)) Set(Mathf.Clamp01(value) * Maximum); }
        private void Set(float value)
        {
            float next = Mathf.Clamp(value, 0f, Maximum);
            if (Mathf.Approximately(next, _sanity)) return;
            _sanity = next;
            Changed?.Invoke(_sanity);
            SanityStage nextStage = EvaluateStage(Normalized);
            if (nextStage == Stage) return;
            Stage = nextStage;
            StageChanged?.Invoke(Stage);
        }

        private SanityStage EvaluateStage(float normalized)
        {
            float uneasy = _profile != null ? _profile.UneasyThreshold : .65f;
            float distressed = _profile != null ? _profile.DistressedThreshold : .35f;
            float critical = _profile != null ? _profile.CriticalThreshold : .15f;
            if (normalized <= critical) return SanityStage.Critical;
            if (normalized <= distressed) return SanityStage.Distressed;
            if (normalized <= uneasy) return SanityStage.Uneasy;
            return SanityStage.Stable;
        }
        public string SaveId => "Sanity";
        public string SaveData() => JsonUtility.ToJson(new SanityState { value = _sanity });
        public void LoadData(string json) { var state = JsonUtility.FromJson<SanityState>(json); if (state != null) Set(state.value); }
    }
}
