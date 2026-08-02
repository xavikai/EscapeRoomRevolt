using System;
using System.Collections;
using EscapeRoomRevolt.Core.Settings;
using EscapeRoomRevolt.Player;
using EscapeRoomRevolt.Player.PC;
using EscapeRoomRevolt.Player.VR;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Survival
{
    /// <summary>Platform-neutral traversal executor shared by PC and VR rigs.</summary>
    [DefaultExecutionOrder(-30)]
    public sealed class TraversalController : MonoBehaviour
    {
        public static TraversalController Instance { get; private set; }

        private PlayerMovement _pcMovement;
        private VRComfortController _vrComfort;
        private VRPlayerPlatformAdapter _vrAdapter;
        private CharacterController _characterController;
        private PlayerVitals _vitals;
        private TraversalObstacle _activeObstacle;
        private Coroutine _routine;
        private Vector3 _safePosition;
        private Quaternion _safeRotation;

        public bool IsTraversing => _routine != null;
        public TraversalObstacle ActiveObstacle => _activeObstacle;
        public event Action<TraversalObstacle> TraversalStarted;
        public event Action<TraversalObstacle> TraversalCompleted;
        public event Action<TraversalObstacle> TraversalCancelled;

        private void Awake()
        {
            if (!GameFeatures.IsEnabled(OptionalGameFeature.Traversal)) { enabled = false; return; }
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            _pcMovement = GetComponent<PlayerMovement>();
            _vrComfort = GetComponent<VRComfortController>();
            _vrAdapter = GetComponent<VRPlayerPlatformAdapter>();
            _characterController = GetComponent<CharacterController>();
            _vitals = GetComponent<PlayerVitals>();
        }

        private void OnEnable()
        {
            if (_vitals == null) _vitals = GetComponent<PlayerVitals>();
            if (_vitals != null) _vitals.Died += HandleDeath;
        }

        private void Start()
        {
            if (CheckpointManager.Instance != null) CheckpointManager.Instance.Respawned += HandleRespawn;
        }

        private void OnDisable()
        {
            if (_vitals != null) _vitals.Died -= HandleDeath;
            if (CheckpointManager.Instance != null) CheckpointManager.Instance.Respawned -= HandleRespawn;
            CancelTraversal();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public bool TryBegin(TraversalObstacle obstacle)
        {
            if (obstacle == null || IsTraversing || !isActiveAndEnabled || (_vitals != null && _vitals.IsDead)) return false;
            _activeObstacle = obstacle;
            _safePosition = transform.position;
            _safeRotation = transform.rotation;
            SetControlsBlocked(true);
            if (_characterController != null) _characterController.enabled = false;
            obstacle.NotifyStarted();
            TraversalStarted?.Invoke(obstacle);
            _routine = StartCoroutine(Traverse(obstacle));
            return true;
        }

        public void CancelTraversal()
        {
            if (!IsTraversing) return;
            TraversalObstacle cancelled = _activeObstacle;
            StopCoroutine(_routine);
            _routine = null;
            MoveRig(_safePosition, _safeRotation);
            ReleaseControls();
            _activeObstacle = null;
            cancelled?.NotifyCancelled();
            TraversalCancelled?.Invoke(cancelled);
        }

        private IEnumerator Traverse(TraversalObstacle obstacle)
        {
            Vector3 startPosition = transform.position;
            Quaternion startRotation = transform.rotation;
            obstacle.ResolvePath(startPosition, out Vector3 entryPosition, out Quaternion entryRotation,
                out Vector3 exitPosition, out Quaternion exitRotation);
            bool instantVR = _vrAdapter != null && _vrComfort != null && _vrComfort.Settings != null
                && _vrComfort.Settings.traversalMode == VRTraversalMode.Instant;
            float durationMultiplier = _vrAdapter != null && _vrComfort != null && _vrComfort.Settings != null
                ? _vrComfort.Settings.traversalDurationMultiplier : 1f;
            float duration = Mathf.Max(.05f, obstacle.Duration * Mathf.Max(.25f, durationMultiplier));
            if (instantVR)
            {
                yield return null;
                if (obstacle == null)
                {
                    AbortDestroyedObstacle();
                    yield break;
                }
                CompleteTraversal(obstacle, exitPosition, exitRotation);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (obstacle == null)
                {
                    AbortDestroyedObstacle();
                    yield break;
                }
                elapsed += Time.unscaledDeltaTime;
                float raw = Mathf.Clamp01(elapsed / duration);
                Vector3 position = obstacle.EvaluatePosition(startPosition, entryPosition, exitPosition, raw);
                Quaternion rotation = obstacle.EvaluateRotation(startRotation, entryRotation, exitRotation, raw);
                MoveRig(position, rotation);
                yield return null;
            }

            CompleteTraversal(obstacle, exitPosition, exitRotation);
        }

        private void CompleteTraversal(TraversalObstacle obstacle, Vector3 exitPosition, Quaternion exitRotation)
        {
            MoveRig(exitPosition, exitRotation);
            _routine = null;
            ReleaseControls();
            _activeObstacle = null;
            obstacle.NotifyCompleted();
            TraversalCompleted?.Invoke(obstacle);
        }

        private void AbortDestroyedObstacle()
        {
            _routine = null;
            MoveRig(_safePosition, _safeRotation);
            ReleaseControls();
            _activeObstacle = null;
            TraversalCancelled?.Invoke(null);
        }

        private void MoveRig(Vector3 position, Quaternion rotation)
        {
            if (_vrAdapter != null) _vrAdapter.TeleportRig(position, rotation);
            else transform.SetPositionAndRotation(position, rotation);
        }

        private void SetControlsBlocked(bool blocked)
        {
            if (_pcMovement != null)
            {
                _pcMovement.IsMovementFrozen = blocked;
                _pcMovement.IsMouseLookFrozen = blocked;
            }
            _vrComfort?.SetMovementBlocked(blocked);
        }

        private void ReleaseControls()
        {
            if (_characterController != null) _characterController.enabled = true;
            if (_vitals == null || !_vitals.IsDead) SetControlsBlocked(false);
        }

        private void HandleDeath() => CancelTraversal();
        private void HandleRespawn() => CancelTraversal();
    }
}
