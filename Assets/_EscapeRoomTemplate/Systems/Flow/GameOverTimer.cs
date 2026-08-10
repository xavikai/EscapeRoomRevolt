using System;
using EscapeRoomRevolt.Core;
using EscapeRoomRevolt.Core.Flow;
using EscapeRoomRevolt.Core.Save;
using UnityEngine;
using UnityEngine.Events;

namespace EscapeRoomRevolt.Systems.Flow
{
    /// <summary>
    /// Independent fail-state countdown. It owns no movement and no UI implementation: timer state
    /// is published through EventBus so the shared gameplay HUD can present it when requested.
    /// </summary>
    public sealed class GameOverTimer : MonoBehaviour, ISaveable
    {
        [Header("Identity")]
        [SerializeField] private string _saveId;

        [Header("Countdown")]
        [SerializeField, Min(.01f)] private float _duration = 60f;
        [SerializeField] private bool _autoStart;

        [Header("HUD")]
        [SerializeField] private bool _showInHud = true;
        [SerializeField] private string _hudLabel = "TEMPS RESTANT";

        [Header("Failure")]
        [SerializeField] private EndingDefinition _ending;

        [Header("Events")]
        [SerializeField] private UnityEvent _onStarted;
        [SerializeField] private UnityEvent _onStopped;
        [SerializeField] private UnityEvent<float> _onTimeRemainingChanged;
        [SerializeField] private UnityEvent _onExpired;

        private float _elapsed;
        private bool _isRunning;
        private bool _hasExpired;

        public string SaveId => string.IsNullOrWhiteSpace(_saveId) ? name : _saveId;
        public float Duration => _duration;
        public float TimeRemaining => Mathf.Max(0f, _duration - _elapsed);
        public float NormalizedRemaining => Mathf.Clamp01(TimeRemaining / Mathf.Max(.01f, _duration));
        public bool IsRunning => _isRunning;
        public bool HasExpired => _hasExpired;
        public bool ShowInHud => _showInHud;

        private void Awake() => SaveManager.Instance?.Register(this);

        private void Start()
        {
            if (_autoStart) StartTimer();
            else PublishState(false);
        }

        private void Update()
        {
            if (_isRunning) AdvanceTime(Time.deltaTime);
        }

        private void OnDisable()
        {
            if (Application.isPlaying) PublishState(false);
        }

        private void OnDestroy()
        {
            PublishState(false);
            SaveManager.Instance?.Unregister(this);
        }

        public void StartTimer()
        {
            if (_hasExpired || _isRunning) return;
            _isRunning = true;
            _onStarted?.Invoke();
            PublishState(_showInHud);
        }

        public void StopTimer()
        {
            if (!_isRunning) return;
            _isRunning = false;
            _onStopped?.Invoke();
            PublishState(false);
        }

        public void ResetTimer()
        {
            _isRunning = false;
            _hasExpired = false;
            _elapsed = 0f;
            _onTimeRemainingChanged?.Invoke(TimeRemaining);
            PublishState(false);
        }

        /// <summary>Deterministic tick used by Update and automated tests.</summary>
        public void AdvanceTime(float deltaTime)
        {
            if (!_isRunning || _hasExpired || deltaTime <= 0f) return;
            _elapsed = Mathf.Min(_duration, _elapsed + deltaTime);
            _onTimeRemainingChanged?.Invoke(TimeRemaining);
            PublishState(_showInHud);
            if (_elapsed >= _duration) Expire();
        }

        public void Expire()
        {
            if (_hasExpired) return;
            _elapsed = _duration;
            _hasExpired = true;
            _isRunning = false;
            _onTimeRemainingChanged?.Invoke(0f);
            _onExpired?.Invoke();
            PublishState(_showInHud);
            GameFlowManager.EnsureInstance().FailGame(_ending);
        }

        private void PublishState(bool visible)
        {
            EventBus.Publish(new OnGameOverTimerChanged
            {
                timerId = SaveId,
                label = string.IsNullOrWhiteSpace(_hudLabel) ? "TEMPS RESTANT" : _hudLabel,
                secondsRemaining = TimeRemaining,
                normalizedRemaining = NormalizedRemaining,
                isRunning = _isRunning,
                isVisible = visible,
                hasExpired = _hasExpired
            });
        }

        [Serializable]
        private sealed class TimerSaveData
        {
            public float elapsed;
            public bool isRunning;
            public bool hasExpired;
        }

        public string SaveData()
        {
            return JsonUtility.ToJson(new TimerSaveData
            {
                elapsed = _elapsed,
                isRunning = _isRunning,
                hasExpired = _hasExpired
            });
        }

        public void LoadData(string json)
        {
            TimerSaveData data = JsonUtility.FromJson<TimerSaveData>(json);
            if (data == null) return;
            _elapsed = Mathf.Clamp(data.elapsed, 0f, _duration);
            _isRunning = data.isRunning && !data.hasExpired;
            _hasExpired = data.hasExpired;
            _onTimeRemainingChanged?.Invoke(TimeRemaining);
            PublishState(_showInHud && (_isRunning || _hasExpired));
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _duration = Mathf.Max(.01f, _duration);
            if (string.IsNullOrWhiteSpace(_saveId)) _saveId = "game_over_timer_" + Guid.NewGuid().ToString("N");
        }
#endif
    }
}
