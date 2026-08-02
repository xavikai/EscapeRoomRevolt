using UnityEngine;

namespace EscapeRoomRevolt.Systems.Survival
{
    [CreateAssetMenu(fileName = "SanityProfile", menuName = "Escape Room Framework/Survival/Sanity Profile", order = 30)]
    public sealed class SanityProfile : ScriptableObject
    {
        [Min(1f)] [SerializeField] private float _maximum = 100f;
        [Min(0f)] [SerializeField] private float _passiveRecoveryPerSecond = 0.35f;
        [Range(0f, 1f)] [SerializeField] private float _uneasyThreshold = 0.65f;
        [Range(0f, 1f)] [SerializeField] private float _distressedThreshold = 0.35f;
        [Range(0f, 1f)] [SerializeField] private float _criticalThreshold = 0.15f;

        public float Maximum => _maximum;
        public float PassiveRecoveryPerSecond => _passiveRecoveryPerSecond;
        public float UneasyThreshold => _uneasyThreshold;
        public float DistressedThreshold => _distressedThreshold;
        public float CriticalThreshold => _criticalThreshold;

        private void OnValidate()
        {
            _maximum = Mathf.Max(1f, _maximum);
            _distressedThreshold = Mathf.Min(_distressedThreshold, _uneasyThreshold);
            _criticalThreshold = Mathf.Min(_criticalThreshold, _distressedThreshold);
        }
    }
}
