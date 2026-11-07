using System;
using EscapeRoomRevolt.Core;
using EscapeRoomRevolt.Core.Input;
using EscapeRoomRevolt.Core.Settings;
using UnityEngine;
using UnityEngine.Events;

namespace EscapeRoomRevolt.Systems.Survival
{
    /// <summary>Centre-screen recording detector, separated from battery and presentation.</summary>
    [RequireComponent(typeof(NightVisionController))]
    public sealed class CamcorderEvidenceRecorder : MonoBehaviour
    {
        [SerializeField] private Camera _viewCamera;
        [SerializeField] private LayerMask _recordingMask = ~0;
        [SerializeField, Min(1f)] private float _rayDistance = 30f;
        [SerializeField] private bool _resetProgressWhenReleased = true;
        [Header("Presentation hooks")]
        [SerializeField] private UnityEvent _onRecordingStarted;
        [SerializeField] private UnityEvent _onRecordingStopped;
        [SerializeField] private UnityEvent _onEvidenceCompleted;

        private NightVisionController _camcorder;
        private RecordableEvidence _currentTarget;
        private float _recordedSeconds;
        private bool _isRecording;

        public RecordableEvidence CurrentTarget => _currentTarget;
        public EvidenceDefinition CurrentDefinition => _currentTarget != null ? _currentTarget.Definition : null;
        public bool IsRecording => _isRecording;
        public float Progress01 => CurrentDefinition != null
            ? Mathf.Clamp01(_recordedSeconds / Mathf.Max(.1f, CurrentDefinition.RecordingSeconds)) : 0f;

        public event Action<RecordableEvidence> FocusChanged;
        public event Action<float> ProgressChanged;
        public event Action<bool> RecordingStateChanged;
        public event Action<EvidenceDefinition> EvidenceCompleted;

        private void Awake()
        {
            _camcorder = GetComponent<NightVisionController>();
            if (!GameFeatures.IsEnabled(OptionalGameFeature.EvidenceRecording)) enabled = false;
        }

        private void Update()
        {
            if (_camcorder == null || !_camcorder.IsCamcorderRaised || IsGameplayBlocked())
            {
                StopRecording(clearFocus: true);
                return;
            }

            _viewCamera = _viewCamera != null ? _viewCamera : _camcorder.ViewCamera;
            RecordableEvidence target = FindTarget();
            SetTarget(target);
            bool held = InputRouter.Instance != null && InputRouter.Instance.RecordEvidenceHeld;
            if (!held || target == null || target.IsRecorded)
            {
                StopRecording(clearFocus: false);
                if (!held && _resetProgressWhenReleased) ResetProgress();
                return;
            }

            TickRecording(target, Time.deltaTime);
        }

        public bool TickRecording(RecordableEvidence target, float deltaTime)
        {
            if (target == null || target.IsRecorded || _camcorder == null || !_camcorder.IsCamcorderRaised) return false;
            SetTarget(target);
            if (!_isRecording)
            {
                _isRecording = true;
                RecordingStateChanged?.Invoke(true);
                _onRecordingStarted?.Invoke();
            }

            _recordedSeconds += Mathf.Max(0f, deltaTime);
            ProgressChanged?.Invoke(Progress01);
            if (Progress01 < 1f) return false;

            EvidenceDefinition completed = target.Definition;
            bool added = EvidenceJournal.EnsureInstance().Record(completed);
            _onEvidenceCompleted?.Invoke();
            EvidenceCompleted?.Invoke(completed);
            StopRecording(clearFocus: false);
            return added;
        }

        public void StopRecording(bool clearFocus)
        {
            if (_isRecording)
            {
                _isRecording = false;
                RecordingStateChanged?.Invoke(false);
                _onRecordingStopped?.Invoke();
            }
            if (clearFocus) SetTarget(null);
        }

        private RecordableEvidence FindTarget()
        {
            if (_viewCamera == null) return null;
            Ray ray = new(_viewCamera.transform.position, _viewCamera.transform.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, _rayDistance, _recordingMask, QueryTriggerInteraction.Collide)) return null;
            RecordableEvidence target = hit.transform.GetComponentInParent<RecordableEvidence>();
            return target != null && target.CanRecordFrom(_viewCamera) ? target : null;
        }

        private void SetTarget(RecordableEvidence value)
        {
            if (_currentTarget == value) return;
            _currentTarget = value;
            _recordedSeconds = 0f;
            FocusChanged?.Invoke(value);
            ProgressChanged?.Invoke(0f);
        }

        private void ResetProgress()
        {
            if (_recordedSeconds <= 0f) return;
            _recordedSeconds = 0f;
            ProgressChanged?.Invoke(0f);
        }

        private static bool IsGameplayBlocked() => GameplayBlockState.IsBlocking;
    }
}
