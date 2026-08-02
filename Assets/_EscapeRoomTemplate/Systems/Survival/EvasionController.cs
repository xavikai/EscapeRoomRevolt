using System;
using EscapeRoomRevolt.Core.Input;
using EscapeRoomRevolt.Core.Settings;
using EscapeRoomRevolt.Player.PC;
using EscapeRoomRevolt.Player.VR;
using EscapeRoomRevolt.UI.PC;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Survival
{
    /// <summary>
    /// Optional Survival Horror evasive movement. PC receives camera-safe lean/look-back and a
    /// collision-driven slide. VR keeps lean/look-back physical and exposes artificial slide only
    /// when the active comfort profile explicitly enables it.
    /// </summary>
    [DefaultExecutionOrder(-25)]
    [RequireComponent(typeof(CharacterController))]
    public sealed class EvasionController : MonoBehaviour
    {
        [Header("Lean (PC)")]
        [SerializeField, Range(.05f, .6f)] private float _leanDistance = .32f;
        [SerializeField, Range(0f, 15f)] private float _leanRoll = 8f;
        [SerializeField, Min(.1f)] private float _leanResponse = 6f;
        [SerializeField, Range(.05f, .3f)] private float _leanCollisionRadius = .14f;

        [Header("Look Back (PC)")]
        [SerializeField, Range(120f, 180f)] private float _lookBackAngle = 165f;
        [SerializeField, Min(30f)] private float _lookBackSpeed = 540f;

        [Header("Slide")]
        [SerializeField, Min(.1f)] private float _slideSpeed = 7.5f;
        [SerializeField, Range(.2f, 2f)] private float _slideDuration = .75f;
        [SerializeField, Range(0f, 1f)] private float _minimumForwardInput = .25f;
        [SerializeField, Min(0f)] private float _slideNoiseRadius = 11f;
        [SerializeField] private AnimationCurve _slideSpeedCurve =
            new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));

        private readonly RaycastHit[] _leanHits = new RaycastHit[16];
        private PlayerMovement _pcMovement;
        private VRPlayerPlatformAdapter _vrAdapter;
        private VRComfortController _vrComfort;
        private CharacterController _controller;
        private PlayerVitals _vitals;
        private TraversalController _traversal;
        private Transform _view;
        private Vector3 _baseViewLocalPosition;
        private float _lean;
        private float _lookBackYaw;
        private float _slideElapsed;
        private Vector3 _slideDirection;
        private bool _leanOverrideActive;
        private float _leanOverride;
        private bool _lookBackOverrideActive;
        private bool _lookBackOverride;
        private bool _ownsPCMovementBlock;
        private bool _ownsVRMovementBlock;

        public bool IsSliding { get; private set; }
        public float LeanAmount => _lean;
        public float LookBackYaw => _lookBackYaw;
        public event Action SlideStarted;
        public event Action SlideCompleted;
        public event Action SlideCancelled;

        private void Awake()
        {
            if (!GameFeatures.IsEnabled(OptionalGameFeature.AdvancedEvasion))
            {
                enabled = false;
                return;
            }

            _controller = GetComponent<CharacterController>();
            _pcMovement = GetComponent<PlayerMovement>();
            _vrAdapter = GetComponent<VRPlayerPlatformAdapter>();
            _vrComfort = GetComponent<VRComfortController>();
            _vitals = GetComponent<PlayerVitals>();
            _traversal = GetComponent<TraversalController>();
            _view = _pcMovement != null ? _pcMovement.ViewTransform : null;
            if (_view != null) _baseViewLocalPosition = _view.localPosition;
        }

        private void OnEnable()
        {
            if (_vitals != null) _vitals.Died += HandleDeath;
            if (_traversal != null) _traversal.TraversalStarted += HandleTraversalStarted;
        }

        private void Start()
        {
            if (CheckpointManager.Instance != null) CheckpointManager.Instance.Respawned += HandleRespawn;
        }

        private void OnDisable()
        {
            if (_vitals != null) _vitals.Died -= HandleDeath;
            if (_traversal != null) _traversal.TraversalStarted -= HandleTraversalStarted;
            if (CheckpointManager.Instance != null) CheckpointManager.Instance.Respawned -= HandleRespawn;
            if (IsSliding) FinishSlide(true, true);
            ResetViewOffsets();
        }

        private void Update()
        {
            InputRouter input = InputRouter.Instance;
            if (IsBlocked())
            {
                if (IsSliding) FinishSlide(true, false);
                return;
            }

            if (IsSliding) UpdateSlide();

            if (!IsSliding && input != null && input.SlidePressed)
            {
                if (_pcMovement != null && input.SprintHeld && input.Move.y >= _minimumForwardInput)
                    TryStartSlide(transform.forward);
                else if (_vrAdapter != null && AllowsVRSlide())
                    TryStartSlide(ResolveVRForward());
            }
        }

        private void LateUpdate()
        {
            if (_pcMovement == null || _view == null) return;

            InputRouter input = InputRouter.Instance;
            bool blocked = IsBlocked() || IsSliding;
            float requestedLean = blocked ? 0f : _leanOverrideActive
                ? _leanOverride
                : input != null && input.LeanModifierHeld ? input.Move.x : 0f;
            bool requestedLookBack = !blocked && (_lookBackOverrideActive
                ? _lookBackOverride
                : input != null && input.LookBackHeld);

            float safeLean = ResolveSafeLean(Mathf.Clamp(requestedLean, -1f, 1f));
            _lean = Mathf.MoveTowards(_lean, safeLean, _leanResponse * Time.deltaTime);
            float targetYaw = requestedLookBack ? _lookBackAngle : 0f;
            _lookBackYaw = Mathf.MoveTowardsAngle(_lookBackYaw, targetYaw, _lookBackSpeed * Time.deltaTime);

            Vector3 localPosition = _view.localPosition;
            localPosition.x = _baseViewLocalPosition.x + _lean * _leanDistance;
            localPosition.z = _baseViewLocalPosition.z;
            _view.localPosition = localPosition;
            _view.localRotation = Quaternion.Euler(_pcMovement.CameraPitch, _lookBackYaw, -_lean * _leanRoll);
        }

        /// <summary>Starts a slide from custom input, AI or tests after applying all safety checks.</summary>
        public bool TryStartSlide(Vector3 worldDirection)
        {
            if (!isActiveAndEnabled || IsSliding || IsBlocked() || _controller == null || !_controller.enabled
                || !_controller.isGrounded || (_vrAdapter != null && !AllowsVRSlide())) return false;

            worldDirection.y = 0f;
            if (worldDirection.sqrMagnitude < .01f) return false;
            _slideDirection = worldDirection.normalized;
            _slideElapsed = 0f;
            IsSliding = true;

            if (_pcMovement != null)
            {
                if (_pcMovement.IsMovementFrozen) { IsSliding = false; return false; }
                _pcMovement.SetEvasionCrouch(true);
                _pcMovement.IsMovementFrozen = true;
                _ownsPCMovementBlock = true;
            }
            if (_vrAdapter != null && _vrComfort != null)
            {
                _vrComfort.SetMovementBlocked(true);
                _ownsVRMovementBlock = true;
            }

            GameplayNoise.Emit(transform.position, _slideNoiseRadius, GameplayNoiseType.PlayerAction, gameObject);
            SlideStarted?.Invoke();
            return true;
        }

        public void CancelSlide()
        {
            if (IsSliding) FinishSlide(true, false);
        }

        /// <summary>Accessibility/test hook. The normal input path remains Alt + movement.</summary>
        public void SetLeanOverride(float normalized)
        {
            _leanOverrideActive = true;
            _leanOverride = Mathf.Clamp(normalized, -1f, 1f);
        }

        public void ClearLeanOverride() => _leanOverrideActive = false;

        /// <summary>Accessibility/test hook for held look-back state.</summary>
        public void SetLookBackOverride(bool active)
        {
            _lookBackOverrideActive = true;
            _lookBackOverride = active;
        }

        public void ClearLookBackOverride() => _lookBackOverrideActive = false;

        private void UpdateSlide()
        {
            if (_controller == null || !_controller.enabled)
            {
                FinishSlide(true, false);
                return;
            }

            _slideElapsed += Time.deltaTime;
            float normalized = Mathf.Clamp01(_slideElapsed / Mathf.Max(.01f, _slideDuration));
            float speedScale = _slideSpeedCurve == null ? 1f - normalized : Mathf.Max(0f, _slideSpeedCurve.Evaluate(normalized));
            float vrScale = _vrAdapter != null && _vrComfort != null && _vrComfort.Settings != null
                ? _vrComfort.Settings.artificialSlideSpeedMultiplier : 1f;
            _controller.Move((_slideDirection * (_slideSpeed * speedScale * vrScale) + Vector3.down * 2f) * Time.deltaTime);
            if (normalized >= 1f) FinishSlide(false, false);
        }

        private void FinishSlide(bool cancelled, bool forceRelease)
        {
            if (!IsSliding) return;
            IsSliding = false;
            _slideElapsed = 0f;
            if (_pcMovement != null) _pcMovement.SetEvasionCrouch(false);

            bool release = forceRelease || CanReleaseControls();
            if (_ownsPCMovementBlock && release && _pcMovement != null) _pcMovement.IsMovementFrozen = false;
            if (_ownsVRMovementBlock && release && _vrComfort != null) _vrComfort.SetMovementBlocked(false);
            _ownsPCMovementBlock = false;
            _ownsVRMovementBlock = false;

            if (cancelled) SlideCancelled?.Invoke();
            else SlideCompleted?.Invoke();
        }

        private float ResolveSafeLean(float requested)
        {
            if (Mathf.Approximately(requested, 0f) || _view.parent == null) return requested;
            float requestedDistance = Mathf.Abs(requested) * _leanDistance;
            Vector3 localBase = _baseViewLocalPosition;
            localBase.y = _view.localPosition.y;
            Vector3 origin = _view.parent.TransformPoint(localBase);
            Vector3 direction = _view.parent.right * Mathf.Sign(requested);
            int count = Physics.SphereCastNonAlloc(origin, _leanCollisionRadius, direction, _leanHits,
                requestedDistance, ~0, QueryTriggerInteraction.Ignore);
            float safeDistance = requestedDistance;
            for (int index = 0; index < count; index++)
            {
                Collider candidate = _leanHits[index].collider;
                if (candidate == null || candidate.transform.IsChildOf(transform)) continue;
                safeDistance = Mathf.Min(safeDistance, Mathf.Max(0f, _leanHits[index].distance - .01f));
            }
            return Mathf.Sign(requested) * (safeDistance / _leanDistance);
        }

        private bool AllowsVRSlide()
        {
            return _vrAdapter == null || (_vrComfort != null && _vrComfort.Settings != null
                && _vrComfort.Settings.allowArtificialSlide);
        }

        private Vector3 ResolveVRForward()
        {
            Vector3 forward = _vrAdapter != null && _vrAdapter.Head != null ? _vrAdapter.Head.forward : transform.forward;
            forward.y = 0f;
            return forward.sqrMagnitude > .01f ? forward.normalized : transform.forward;
        }

        private bool IsBlocked()
        {
            bool uiBlocking = (UIManager.Instance != null && UIManager.Instance.IsUIBlockingGameplay)
                || (EscapeRoomRevolt.UI.Toolkit.UIToolkitMenuController.Instance != null
                    && EscapeRoomRevolt.UI.Toolkit.UIToolkitMenuController.Instance.IsBlockingGameplay);
            return uiBlocking || (_vitals != null && (_vitals.IsDead || _vitals.IsHidden))
                || (_traversal != null && _traversal.IsTraversing);
        }

        private bool CanReleaseControls()
        {
            return (_vitals == null || (!_vitals.IsDead && !_vitals.IsHidden))
                && (_traversal == null || !_traversal.IsTraversing);
        }

        private void ResetViewOffsets()
        {
            if (_view == null) return;
            Vector3 local = _view.localPosition;
            local.x = _baseViewLocalPosition.x;
            local.z = _baseViewLocalPosition.z;
            _view.localPosition = local;
            Vector3 angles = _view.localEulerAngles;
            _view.localRotation = Quaternion.Euler(angles.x, 0f, 0f);
            _lean = 0f;
            _lookBackYaw = 0f;
        }

        private void HandleDeath()
        {
            if (IsSliding) FinishSlide(true, false);
        }

        private void HandleRespawn()
        {
            if (IsSliding) FinishSlide(true, true);
            ResetViewOffsets();
        }

        private void HandleTraversalStarted(TraversalObstacle obstacle)
        {
            if (IsSliding) FinishSlide(true, false);
        }
    }
}
