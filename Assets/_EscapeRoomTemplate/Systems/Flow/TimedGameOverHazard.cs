using System;
using EscapeRoomRevolt.Core.Flow;
using EscapeRoomRevolt.Core.Save;
using UnityEngine;
using UnityEngine.Events;

namespace EscapeRoomRevolt.Systems.Flow
{
    /// <summary>
    /// Generic moving threat for descending walls, rising water, crushing platforms or closing
    /// rooms. Movement and the optional independent countdown can each end the game.
    /// </summary>
    public sealed class TimedGameOverHazard : MonoBehaviour, ISaveable
    {
        [Header("Identity")]
        [SerializeField] private string _saveId;

        [Header("Movement")]
        [SerializeField] private Transform _startPoint;
        [SerializeField] private Transform _endPoint;
        [SerializeField, Min(.01f)] private float _travelDuration = 20f;
        [SerializeField] private bool _autoStart;
        [SerializeField] private bool _failAtDestination = true;

        [Header("Optional Countdown")]
        [SerializeField] private bool _countdownEnabled;
        [SerializeField, Min(.01f)] private float _countdownDuration = 60f;

        [Header("Contact")]
        [SerializeField] private bool _failOnPlayerContact = true;
        [SerializeField] private string _playerTag = "Player";

        [Header("Failure")]
        [SerializeField] private EndingDefinition _ending;

        [Header("Events")]
        [SerializeField] private UnityEvent _onStarted;
        [SerializeField] private UnityEvent _onStopped;
        [SerializeField] private UnityEvent<float> _onProgressChanged;
        [SerializeField] private UnityEvent<float> _onTimeRemainingChanged;
        [SerializeField] private UnityEvent _onFailed;

        private float _elapsedTravel;
        private float _elapsedCountdown;
        private bool _isRunning;
        private bool _hasFailed;

        public string SaveId => string.IsNullOrWhiteSpace(_saveId) ? name : _saveId;
        public bool IsRunning => _isRunning;
        public bool HasFailed => _hasFailed;
        public bool CountdownEnabled => _countdownEnabled;
        public float Progress => Mathf.Clamp01(_elapsedTravel / Mathf.Max(.01f, _travelDuration));
        public float TimeRemaining => _countdownEnabled
            ? Mathf.Max(0f, _countdownDuration - _elapsedCountdown)
            : Mathf.Max(0f, _travelDuration - _elapsedTravel);

        private void Awake()
        {
            SaveManager.Instance?.Register(this);
            ApplyPosition();
        }

        private void Start()
        {
            if (_autoStart) StartHazard();
        }

        private void OnDestroy()
        {
            SaveManager.Instance?.Unregister(this);
        }

        private void Update()
        {
            if (_isRunning) AdvanceTime(Time.deltaTime);
        }

        public void StartHazard()
        {
            if (_hasFailed || _isRunning) return;
            _isRunning = true;
            _onStarted?.Invoke();
        }

        public void StopHazard()
        {
            if (!_isRunning) return;
            _isRunning = false;
            _onStopped?.Invoke();
        }

        public void ResetHazard()
        {
            _isRunning = false;
            _hasFailed = false;
            _elapsedTravel = 0f;
            _elapsedCountdown = 0f;
            ApplyPosition();
            NotifyProgress();
        }

        /// <summary>Deterministic tick used by Update and by automated tests.</summary>
        public void AdvanceTime(float deltaTime)
        {
            if (!_isRunning || _hasFailed || deltaTime <= 0f) return;

            _elapsedTravel = Mathf.Min(_travelDuration, _elapsedTravel + deltaTime);
            if (_countdownEnabled)
                _elapsedCountdown = Mathf.Min(_countdownDuration, _elapsedCountdown + deltaTime);

            ApplyPosition();
            NotifyProgress();

            bool destinationReached = _failAtDestination && Progress >= 1f;
            bool countdownExpired = _countdownEnabled && _elapsedCountdown >= _countdownDuration;
            if (destinationReached || countdownExpired) TriggerGameOver();
        }

        public void TriggerGameOver()
        {
            if (_hasFailed) return;
            _hasFailed = true;
            _isRunning = false;
            _onFailed?.Invoke();
            GameFlowManager.EnsureInstance().FailGame(_ending);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isRunning || !_failOnPlayerContact || other == null) return;
            if (string.IsNullOrWhiteSpace(_playerTag) || other.CompareTag(_playerTag)) TriggerGameOver();
        }

        private void ApplyPosition()
        {
            if (_startPoint == null || _endPoint == null) return;
            transform.position = Vector3.Lerp(_startPoint.position, _endPoint.position, Progress);
        }

        private void NotifyProgress()
        {
            _onProgressChanged?.Invoke(Progress);
            _onTimeRemainingChanged?.Invoke(TimeRemaining);
        }

        [Serializable]
        private sealed class HazardSaveData
        {
            public float elapsedTravel;
            public float elapsedCountdown;
            public bool isRunning;
            public bool hasFailed;
        }

        public string SaveData()
        {
            return JsonUtility.ToJson(new HazardSaveData
            {
                elapsedTravel = _elapsedTravel,
                elapsedCountdown = _elapsedCountdown,
                isRunning = _isRunning,
                hasFailed = _hasFailed
            });
        }

        public void LoadData(string json)
        {
            HazardSaveData data = JsonUtility.FromJson<HazardSaveData>(json);
            if (data == null) return;
            _elapsedTravel = Mathf.Clamp(data.elapsedTravel, 0f, _travelDuration);
            _elapsedCountdown = Mathf.Clamp(data.elapsedCountdown, 0f, _countdownDuration);
            _isRunning = data.isRunning;
            _hasFailed = data.hasFailed;
            ApplyPosition();
            NotifyProgress();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _travelDuration = Mathf.Max(.01f, _travelDuration);
            _countdownDuration = Mathf.Max(.01f, _countdownDuration);
            if (string.IsNullOrWhiteSpace(_saveId)) _saveId = "hazard_" + Guid.NewGuid().ToString("N");
            if (!Application.isPlaying) ApplyPosition();
        }
#endif
    }
}
