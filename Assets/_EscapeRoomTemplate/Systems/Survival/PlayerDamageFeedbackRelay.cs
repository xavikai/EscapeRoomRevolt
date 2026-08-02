using UnityEngine;
using UnityEngine.Events;

namespace EscapeRoomRevolt.Systems.Survival
{
    /// <summary>Inspector-facing hooks for user-authored visual, audio and haptic feedback.</summary>
    [RequireComponent(typeof(PlayerVitals))]
    public sealed class PlayerDamageFeedbackRelay : MonoBehaviour
    {
        [SerializeField] private UnityEvent<float> _onDamaged;
        [SerializeField] private UnityEvent _onDied;
        [SerializeField] private UnityEvent _onRespawned;
        [SerializeField] private UnityEvent _onDefeat;
        private PlayerVitals _vitals;

        private void Awake() => _vitals = GetComponent<PlayerVitals>();

        private void OnEnable()
        {
            if (_vitals == null) _vitals = GetComponent<PlayerVitals>();
            _vitals.Damaged += HandleDamaged;
            _vitals.Died += HandleDied;
            _vitals.DeathResolved += HandleDeathResolved;
        }

        private void OnDisable()
        {
            if (_vitals == null) return;
            _vitals.Damaged -= HandleDamaged;
            _vitals.Died -= HandleDied;
            _vitals.DeathResolved -= HandleDeathResolved;
        }

        private void HandleDamaged(float amount) => _onDamaged?.Invoke(amount);
        private void HandleDied() => _onDied?.Invoke();
        private void HandleDeathResolved(bool respawned)
        {
            if (respawned) _onRespawned?.Invoke();
            else _onDefeat?.Invoke();
        }
    }
}
