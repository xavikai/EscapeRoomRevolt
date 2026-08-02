using UnityEngine;

namespace EscapeRoomRevolt.Systems.Survival
{
    public enum DamageApplicationMode { OnEnter, Continuous }

    /// <summary>Reusable environmental or trap damage source with no presentation dependency.</summary>
    [RequireComponent(typeof(Collider))]
    public sealed class DamageVolume : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _damage = 20f;
        [SerializeField] private DamageType _damageType = DamageType.Environment;
        [SerializeField] private DamageApplicationMode _mode = DamageApplicationMode.OnEnter;
        [SerializeField, Min(.05f)] private float _repeatInterval = 1f;
        [SerializeField] private bool _disableAfterSuccessfulHit;
        private float _nextDamageAt;

        private void Awake() => GetComponent<Collider>().isTrigger = true;

        private void OnTriggerEnter(Collider other)
        {
            if (_mode == DamageApplicationMode.OnEnter) TryDamage(other);
        }

        private void OnTriggerStay(Collider other)
        {
            if (_mode == DamageApplicationMode.Continuous) TryDamage(other);
        }

        public bool ApplyTo(PlayerVitals vitals)
        {
            if (vitals == null || !isActiveAndEnabled || Time.time < _nextDamageAt) return false;
            _nextDamageAt = Time.time + _repeatInterval;
            vitals.ApplyDamage(new DamageInfo(_damage, _damageType, gameObject, transform.position));
            if (_disableAfterSuccessfulHit) gameObject.SetActive(false);
            return true;
        }

        private void TryDamage(Collider other)
        {
            PlayerVitals vitals = other.GetComponentInParent<PlayerVitals>();
            ApplyTo(vitals);
        }

        private void OnDrawGizmosSelected()
        {
            Collider volume = GetComponent<Collider>();
            if (volume == null) return;
            Gizmos.color = new Color(1f, .1f, .05f, .3f);
            Gizmos.DrawWireCube(volume.bounds.center, volume.bounds.size);
        }
    }
}
