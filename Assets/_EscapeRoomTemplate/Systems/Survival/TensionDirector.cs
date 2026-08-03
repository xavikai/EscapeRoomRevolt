using System;
using System.Collections.Generic;
using EscapeRoomRevolt.Core.Settings;
using UnityEngine;
using UnityEngine.Events;

namespace EscapeRoomRevolt.Systems.Survival
{
    [Serializable] public sealed class HorrorEventDefinitionEvent : UnityEvent<HorrorEventDefinition> { }

    /// <summary>
    /// Optional global pacing gate for HorrorEventTrigger. Enforces a minimum spacing between any
    /// two events (not just repeats of the same one), a rolling budget so scares don't stack up,
    /// and a safe-zone grace window after respawns or ChaseSafeZone crossings. Add one instance to
    /// a scene to opt in; with no TensionDirector present, HorrorEventTrigger behaves exactly as
    /// before. Does not choose what to trigger or replace per-event cooldown/sanity gating — the
    /// level author still places and tunes every HorrorEventTrigger.
    /// </summary>
    [DefaultExecutionOrder(-40)]
    public sealed class TensionDirector : MonoBehaviour
    {
        public static TensionDirector Instance { get; private set; }

        [Header("Pacing")]
        [Tooltip("Minimum seconds between any two horror events, regardless of which trigger fires.")]
        [SerializeField, Min(0f)] private float _globalCooldown = 25f;
        [Tooltip("Maximum horror events allowed within the rolling budget window below.")]
        [SerializeField, Min(1)] private int _maxEventsPerWindow = 3;
        [SerializeField, Min(1f)] private float _budgetWindowSeconds = 300f;

        [Header("Safe Zones")]
        [Tooltip("Seconds of guaranteed quiet granted after the player respawns at a checkpoint.")]
        [SerializeField, Min(0f)] private float _respawnGraceSeconds = 12f;

        [Header("Designer Hooks")]
        [SerializeField] private HorrorEventDefinitionEvent _onEventPermitted;
        [SerializeField] private HorrorEventDefinitionEvent _onEventDenied;

        private readonly Queue<float> _recentEventTimes = new Queue<float>();
        private float _lastEventAt = float.NegativeInfinity;
        private float _suppressedUntil = float.NegativeInfinity;

        public event Action<HorrorEventDefinition> EventPermitted;
        public event Action<HorrorEventDefinition> EventDenied;

        private void Awake()
        {
            if (!GameFeatures.IsEnabled(OptionalGameFeature.HorrorEvents)) { enabled = false; return; }
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            if (CheckpointManager.Instance != null) CheckpointManager.Instance.Respawned += HandleRespawned;
        }

        private void OnDisable()
        {
            if (CheckpointManager.Instance != null) CheckpointManager.Instance.Respawned -= HandleRespawned;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void HandleRespawned() => SuppressFor(_respawnGraceSeconds);

        /// <summary>Extends the current safe window; call from checkpoints, ChaseSafeZone, or scripted lulls.</summary>
        public void SuppressFor(float seconds)
        {
            float until = Time.unscaledTime + Mathf.Max(0f, seconds);
            if (until > _suppressedUntil) _suppressedUntil = until;
        }

        /// <summary>
        /// Gates one horror event against the global cooldown, rolling budget, and any active safe
        /// window. Called by HorrorEventTrigger after its own per-definition cooldown/sanity checks
        /// already pass, so this only ever makes triggering stricter, never looser.
        /// </summary>
        public bool RequestPermission(HorrorEventDefinition definition)
        {
            float now = Time.unscaledTime;
            TrimExpiredEvents(now);

            bool allowed = now >= _suppressedUntil
                && now - _lastEventAt >= _globalCooldown
                && _recentEventTimes.Count < _maxEventsPerWindow;

            if (allowed)
            {
                _lastEventAt = now;
                _recentEventTimes.Enqueue(now);
                EventPermitted?.Invoke(definition);
                _onEventPermitted?.Invoke(definition);
            }
            else
            {
                EventDenied?.Invoke(definition);
                _onEventDenied?.Invoke(definition);
            }
            return allowed;
        }

        private void TrimExpiredEvents(float now)
        {
            while (_recentEventTimes.Count > 0 && now - _recentEventTimes.Peek() > _budgetWindowSeconds)
                _recentEventTimes.Dequeue();
        }
    }
}
