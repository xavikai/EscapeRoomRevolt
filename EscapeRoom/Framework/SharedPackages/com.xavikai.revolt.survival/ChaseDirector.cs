using System;
using System.Collections.Generic;
using EscapeRoomRevolt.Core.Settings;
using UnityEngine;
using UnityEngine.Events;

namespace EscapeRoomRevolt.Systems.Survival
{
    /// <summary>Aggregates enemy chase state and exposes clean hooks for UI, music and level scripting.</summary>
    [DefaultExecutionOrder(-45)]
    public sealed class ChaseDirector : MonoBehaviour
    {
        public static ChaseDirector Instance { get; private set; }
        [SerializeField, Min(0f)] private float _endGracePeriod = 1.5f;
        [SerializeField] private UnityEvent _onChaseStarted;
        [SerializeField] private UnityEvent _onChaseEnded;

        private readonly HashSet<HorrorEnemyController> _registered = new HashSet<HorrorEnemyController>();
        private readonly HashSet<HorrorEnemyController> _chasers = new HashSet<HorrorEnemyController>();
        private readonly Dictionary<HorrorEnemyController, Action<bool>> _handlers =
            new Dictionary<HorrorEnemyController, Action<bool>>();
        private float _pendingEndAt = float.PositiveInfinity;

        public bool IsChaseActive { get; private set; }
        public int ActiveChasers => _chasers.Count;
        public event Action ChaseStarted;
        public event Action ChaseEnded;

        private void Awake()
        {
            if (!GameFeatures.IsEnabled(OptionalGameFeature.EnemyAI)) { enabled = false; return; }
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Update()
        {
            if (!IsChaseActive || _chasers.Count > 0 || Time.time < _pendingEndAt) return;
            IsChaseActive = false;
            _pendingEndAt = float.PositiveInfinity;
            ChaseEnded?.Invoke();
            _onChaseEnded?.Invoke();
        }

        private void OnDestroy()
        {
            foreach (HorrorEnemyController enemy in _registered)
                if (enemy != null && _handlers.TryGetValue(enemy, out Action<bool> handler))
                    enemy.ChaseChanged -= handler;
            _handlers.Clear();
            if (Instance == this) Instance = null;
        }

        public void Register(HorrorEnemyController enemy)
        {
            if (enemy == null || !_registered.Add(enemy)) return;
            Action<bool> handler = value => HandleChaseChanged(enemy, value);
            _handlers.Add(enemy, handler);
            enemy.ChaseChanged += handler;
            if (enemy.IsChasing) HandleChaseChanged(enemy, true);
        }

        public void Unregister(HorrorEnemyController enemy)
        {
            if (enemy == null || !_registered.Remove(enemy)) return;
            if (_handlers.TryGetValue(enemy, out Action<bool> handler))
            {
                enemy.ChaseChanged -= handler;
                _handlers.Remove(enemy);
            }
            _chasers.Remove(enemy);
            EvaluateEnd();
        }

        public void EndAllChases(float suppressDetectionSeconds)
        {
            HorrorEnemyController[] enemies = new HorrorEnemyController[_registered.Count];
            _registered.CopyTo(enemies);
            foreach (HorrorEnemyController enemy in enemies)
                enemy?.ForceLosePlayer(suppressDetectionSeconds);
        }

        private void HandleChaseChanged(HorrorEnemyController enemy, bool chasing)
        {
            if (enemy == null) return;
            if (chasing)
            {
                _chasers.Add(enemy);
                _pendingEndAt = float.PositiveInfinity;
                if (IsChaseActive) return;
                IsChaseActive = true;
                ChaseStarted?.Invoke();
                _onChaseStarted?.Invoke();
            }
            else
            {
                _chasers.Remove(enemy);
                EvaluateEnd();
            }
        }

        private void EvaluateEnd()
        {
            if (IsChaseActive && _chasers.Count == 0)
                _pendingEndAt = Time.time + _endGracePeriod;
        }
    }

}
