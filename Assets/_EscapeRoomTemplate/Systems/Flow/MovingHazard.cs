using System;
using EscapeRoomRevolt.Core.Flow;
using EscapeRoomRevolt.Core.Save;
using UnityEngine;
using UnityEngine.Events;

namespace EscapeRoomRevolt.Systems.Flow
{
    /// <summary>
    /// Moves a lethal object between two arbitrary world-space markers. The path may point forward,
    /// sideways, vertically or diagonally, so the same component serves walls, ceilings, floors,
    /// platforms and flooding volumes. It contains no countdown logic.
    /// </summary>
    public sealed class MovingHazard : MonoBehaviour, ISaveable
    {
        [Header("Identity")]
        [SerializeField] private string _saveId;

        [Header("Path")]
        [SerializeField] private Transform _startPoint;
        [SerializeField] private Transform _endPoint;
        [SerializeField, Min(.01f)] private float _travelDuration = 20f;
        [SerializeField] private AnimationCurve _motionCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField] private bool _autoStart;

        [Header("Failure")]
        [SerializeField] private bool _failAtDestination = true;
        [SerializeField] private bool _failOnPlayerContact = true;
        [SerializeField] private string _playerTag = "Player";
        [SerializeField] private EndingDefinition _ending;

        [Header("Events")]
        [SerializeField] private UnityEvent _onStarted;
        [SerializeField] private UnityEvent _onStopped;
        [SerializeField] private UnityEvent<float> _onProgressChanged;
        [SerializeField] private UnityEvent _onReachedDestination;
        [SerializeField] private UnityEvent _onFailed;

        private float _elapsedTravel;
        private bool _isRunning;
        private bool _hasReachedDestination;
        private bool _hasFailed;

        public string SaveId => string.IsNullOrWhiteSpace(_saveId) ? name : _saveId;
        public bool IsRunning => _isRunning;
        public bool HasReachedDestination => _hasReachedDestination;
        public bool HasFailed => _hasFailed;
        public float Progress => Mathf.Clamp01(_elapsedTravel / Mathf.Max(.01f, _travelDuration));
        public Vector3 Direction => _startPoint != null && _endPoint != null
            ? (_endPoint.position - _startPoint.position).normalized
            : Vector3.zero;

        private void Awake()
        {
            SaveManager.Instance?.Register(this);
            ApplyPosition();
        }

        private void Start()
        {
            if (_autoStart) StartHazard();
        }

        private void Update()
        {
            if (_isRunning) AdvanceTime(Time.deltaTime);
        }

        private void OnDestroy() => SaveManager.Instance?.Unregister(this);

        public void StartHazard()
        {
            if (_hasFailed || _hasReachedDestination || _isRunning) return;
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
            _elapsedTravel = 0f;
            _isRunning = false;
            _hasReachedDestination = false;
            _hasFailed = false;
            ApplyPosition();
            _onProgressChanged?.Invoke(Progress);
        }

        /// <summary>Deterministic tick used by Update and automated tests.</summary>
        public void AdvanceTime(float deltaTime)
        {
            if (!_isRunning || _hasFailed || _hasReachedDestination || deltaTime <= 0f) return;
            _elapsedTravel = Mathf.Min(_travelDuration, _elapsedTravel + deltaTime);
            ApplyPosition();
            _onProgressChanged?.Invoke(Progress);
            if (Progress < 1f) return;

            _hasReachedDestination = true;
            _isRunning = false;
            _onReachedDestination?.Invoke();
            if (_failAtDestination) TriggerGameOver();
            else _onStopped?.Invoke();
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
            float curvedProgress = _motionCurve != null ? _motionCurve.Evaluate(Progress) : Progress;
            transform.position = Vector3.LerpUnclamped(_startPoint.position, _endPoint.position, curvedProgress);
        }

        [Serializable]
        private sealed class MovingHazardSaveData
        {
            public float elapsedTravel;
            public bool isRunning;
            public bool hasReachedDestination;
            public bool hasFailed;
        }

        public string SaveData()
        {
            return JsonUtility.ToJson(new MovingHazardSaveData
            {
                elapsedTravel = _elapsedTravel,
                isRunning = _isRunning,
                hasReachedDestination = _hasReachedDestination,
                hasFailed = _hasFailed
            });
        }

        public void LoadData(string json)
        {
            MovingHazardSaveData data = JsonUtility.FromJson<MovingHazardSaveData>(json);
            if (data == null) return;
            _elapsedTravel = Mathf.Clamp(data.elapsedTravel, 0f, _travelDuration);
            _isRunning = data.isRunning;
            _hasReachedDestination = data.hasReachedDestination;
            _hasFailed = data.hasFailed;
            ApplyPosition();
            _onProgressChanged?.Invoke(Progress);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _travelDuration = Mathf.Max(.01f, _travelDuration);
            if (string.IsNullOrWhiteSpace(_saveId)) _saveId = "moving_hazard_" + Guid.NewGuid().ToString("N");
            if (!Application.isPlaying) ApplyPosition();
        }

        private void OnDrawGizmosSelected()
        {
            if (_startPoint == null || _endPoint == null) return;
            Gizmos.color = new Color(1f, .28f, .12f, .9f);
            Gizmos.DrawLine(_startPoint.position, _endPoint.position);
            Gizmos.DrawWireSphere(_startPoint.position, .12f);
            Gizmos.DrawSphere(_endPoint.position, .12f);
        }
#endif
    }
}
