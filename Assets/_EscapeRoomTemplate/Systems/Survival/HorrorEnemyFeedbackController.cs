using System;
using UnityEngine;
using UnityEngine.Events;

namespace EscapeRoomRevolt.Systems.Survival
{
    [Serializable]
    public sealed class HorrorEnemyStateEvent : UnityEvent<HorrorEnemyState> { }

    /// <summary>
    /// Turns HorrorEnemyController's StateChanged into positional audio "tells" (growl on
    /// suspicion, snarl on chase, etc.) and exposes a UnityEvent hook so buyers can layer their
    /// own animation/particle/haptic feedback. Clips are optional per state — leave any blank to
    /// skip. Add alongside HorrorEnemyController on any enemy prefab; no other wiring required.
    /// </summary>
    [RequireComponent(typeof(HorrorEnemyController))]
    public sealed class HorrorEnemyFeedbackController : MonoBehaviour
    {
        [Header("State Tells (optional — assign your own SFX)")]
        [SerializeField] private AudioClip _suspiciousClip;
        [SerializeField] private AudioClip _investigateClip;
        [SerializeField] private AudioClip _searchClip;
        [SerializeField] private AudioClip _chaseClip;
        [Tooltip("Played when the enemy gives up and returns to Patrol/Idle from an alert state.")]
        [SerializeField] private AudioClip _standDownClip;
        [SerializeField, Range(0f, 1f)] private float _volume = 1f;
        [SerializeField, Range(0f, .3f)] private float _pitchVariance = .05f;

        [Header("Designer Hooks")]
        [SerializeField] private HorrorEnemyStateEvent _onStateChanged;

        private HorrorEnemyController _enemy;
        private HorrorEnemyState _previousState;

        private void Awake() => _enemy = GetComponent<HorrorEnemyController>();

        private void OnEnable()
        {
            if (_enemy == null) _enemy = GetComponent<HorrorEnemyController>();
            _enemy.StateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            if (_enemy != null) _enemy.StateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(HorrorEnemyState state)
        {
            bool wasAlert = _previousState is HorrorEnemyState.Suspicious or HorrorEnemyState.Investigate
                or HorrorEnemyState.Search or HorrorEnemyState.Chase;
            _previousState = state;

            AudioClip clip = state switch
            {
                HorrorEnemyState.Suspicious => _suspiciousClip,
                HorrorEnemyState.Investigate => _investigateClip,
                HorrorEnemyState.Search => _searchClip,
                HorrorEnemyState.Chase => _chaseClip,
                HorrorEnemyState.Patrol or HorrorEnemyState.Idle or HorrorEnemyState.Return when wasAlert => _standDownClip,
                _ => null
            };

            if (clip != null && EscapeRoomRevolt.Systems.Audio.AudioManager.Instance != null)
                EscapeRoomRevolt.Systems.Audio.AudioManager.Instance.PlaySoundAt(clip, transform.position, _volume, _pitchVariance);

            _onStateChanged?.Invoke(state);
        }
    }
}
