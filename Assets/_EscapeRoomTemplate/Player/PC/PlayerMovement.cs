using UnityEngine;
using EscapeRoomRevolt.UI.PC;
using EscapeRoomRevolt.Core.Save;
using EscapeRoomRevolt.Core.Settings;
using EscapeRoomRevolt.Systems.Survival;

namespace EscapeRoomRevolt.Player.PC
{
    /// <summary>
    /// First-person player movement for PC.
    /// Handles WASD walking, sprint, gravity and mouse look.
    ///
    /// Requirements:
    ///   - CharacterController component on the same GameObject
    ///   - Camera as a child GameObject (assign _playerCamera in the Inspector)
    ///   - UIManager in the scene (used to block movement when UI is open)
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour, ISaveable
    {
        // ── Inspector ────────────────────────────────────────────────────────
        [Header("Movement")]
        [SerializeField] private float _walkSpeed    = 3.5f;
        [SerializeField] private float _sprintSpeed  = 6.0f;
        [SerializeField] private float _gravity      = -9.81f;

        [Header("Jumping & Crouching")]
        [SerializeField] private bool  _canJump      = true;
        [SerializeField] private float _jumpHeight   = 0.5f;
        [SerializeField] private bool  _canCrouch    = true;
        [SerializeField] private float _crouchSpeed  = 2.0f;
        [SerializeField] private float _crouchHeight = 1.0f;
        [SerializeField] private float _standingHeight = 2.0f;
        [SerializeField] private float _crouchTransitionSpeed = 10f;

        [Header("Footsteps")]
        [SerializeField] private EscapeRoomRevolt.Systems.Audio.SurfaceAudioData _surfaceAudioData;
        [SerializeField] private float _footstepDistanceWalk = 1.8f;
        [SerializeField] private float _footstepDistanceSprint = 2.4f;

        [Header("Mouse Look")]
        [SerializeField] private Transform _playerCamera;
        [SerializeField] private float _mouseSensitivity = 120f;
        [SerializeField] private float _maxLookAngle     = 85f;

        // ── Private State ────────────────────────────────────────────────────
        private CharacterController _cc;
        private float  _verticalVelocity;
        private float  _cameraPitch; // Up/down rotation accumulated
        private float  _accumulatedDistance;
        private bool   _isCrouching;
        private bool   _forcedCrouch;
        private bool   _evasionCrouch;
        private Vector3 _originalCameraLocalPosition;
        private PlayerVitals _vitals;
        private readonly Collider[] _stanceOverlaps = new Collider[16];

        public bool IsMovementFrozen { get; set; }
        public bool IsMouseLookFrozen { get; set; }
        public bool IsSprinting { get; private set; }
        public bool IsCrouching => _isCrouching;
        public Transform ViewTransform => _playerCamera;
        public float CameraPitch => _cameraPitch;

        /// <summary>Base sensitivity multiplied by the player's saved accessibility preference (0.1-3, default 1), so the settings slider scales this value instead of replacing it.</summary>
        private float EffectiveMouseSensitivity => _mouseSensitivity * (GameSettingsService.Instance != null ? GameSettingsService.Instance.Data.mouseSensitivity : 1f);

        // ── Unity Lifecycle ──────────────────────────────────────────────────
        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _vitals = GetComponent<PlayerVitals>();

            if (_playerCamera == null)
            {
                // Try to find the camera as a child if not assigned
                Camera cam = GetComponentInChildren<Camera>();
                if (cam != null) _playerCamera = cam.transform;
                else Debug.LogError("[PlayerMovement] No camera assigned or found as a child!", this);
            }

            if (_playerCamera != null)
            {
                _originalCameraLocalPosition = _playerCamera.localPosition;
            }

            SaveManager.Instance?.Register(this);
        }

        private void OnDestroy()
        {
            SaveManager.Instance?.Unregister(this);
        }

        private void Update()
        {
            // Block all input when a UI panel is open
            bool uiBlocking = (UIManager.Instance != null && UIManager.Instance.IsUIBlockingGameplay)
                || (EscapeRoomRevolt.UI.Toolkit.UIToolkitMenuController.Instance != null
                    && EscapeRoomRevolt.UI.Toolkit.UIToolkitMenuController.Instance.IsBlockingGameplay);
            if (uiBlocking)
            {
                IsSprinting = false;
                _vitals?.SetSprinting(false);
                return;
            }

            HandleMouseLook();
            if (IsMovementFrozen)
            {
                IsSprinting = false;
                _vitals?.SetSprinting(false);
                return;
            }
            HandleMovement();
        }

        // ── Private Methods ──────────────────────────────────────────────────
        private void HandleMouseLook()
        {
            if (IsMouseLookFrozen) return;

            var input = EscapeRoomRevolt.Core.Input.InputRouter.Instance;
            Vector2 look = input != null ? input.Look * 0.05f : Vector2.zero;
            float sensitivity = EffectiveMouseSensitivity;
            float mouseX = look.x * sensitivity * Time.deltaTime;
            float mouseY = look.y * sensitivity * Time.deltaTime;

            // Horizontal rotation — rotates the whole player body
            transform.Rotate(Vector3.up * mouseX);

            // Vertical rotation — only the camera, clamped to avoid flipping
            _cameraPitch -= mouseY;
            _cameraPitch  = Mathf.Clamp(_cameraPitch, -_maxLookAngle, _maxLookAngle);
            _playerCamera.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
        }

        private void HandleMovement()
        {
            var input = EscapeRoomRevolt.Core.Input.InputRouter.Instance;
            // Crouching Input
            if (_canCrouch)
            {
                bool wantsCrouch = _forcedCrouch || _evasionCrouch || (input != null && input.CrouchHeld);
                if (!wantsCrouch && _isCrouching && !CanStand()) wantsCrouch = true;
                _isCrouching = wantsCrouch;
            }

            // Smoothly adjust CharacterController height and center
            float targetHeight = _isCrouching ? _crouchHeight : _standingHeight;
            ApplyStance(targetHeight, false);

            // Horizontal movement
            Vector2 moveInput = input != null ? input.Move : Vector2.zero;
            float h = moveInput.x;
            float v = moveInput.y;
            if (GameFeatures.IsEnabled(OptionalGameFeature.AdvancedEvasion)
                && input != null && input.LeanModifierHeld) h = 0f;

            IsSprinting = input != null && input.SprintHeld && !_isCrouching && (_vitals == null || _vitals.CanSprint) && moveInput.sqrMagnitude > .01f;
            _vitals?.SetSprinting(IsSprinting);
            float speed = _isCrouching ? _crouchSpeed : (IsSprinting ? _sprintSpeed : _walkSpeed);

            Vector3 move = transform.right * h + transform.forward * v;
            move = Vector3.ClampMagnitude(move, 1f); // Prevents diagonal speed boost

            // Calculate horizontal distance moved this frame for footsteps
            Vector3 horizontalVelocity = move * speed;
            if (_cc.isGrounded && horizontalVelocity.sqrMagnitude > 0.1f)
            {
                _accumulatedDistance += horizontalVelocity.magnitude * Time.deltaTime;
                float currentStepThreshold = IsSprinting ? _footstepDistanceSprint : _footstepDistanceWalk;

                if (_accumulatedDistance >= currentStepThreshold)
                {
                    _accumulatedDistance = 0f;
                    PlayFootstepSound();
                    float noiseRadius = _isCrouching ? 2.5f : IsSprinting ? 10f : 5f;
                    GameplayNoise.Emit(transform.position, noiseRadius,
                        IsSprinting ? GameplayNoiseType.Sprint : GameplayNoiseType.Footstep, gameObject);
                }
            }
            else
            {
                _accumulatedDistance = 0f; // Reset if stopped or jumping
            }

            // Gravity & Jumping
            if (_cc.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f; // Small negative to keep grounded

            if (_canJump && _cc.isGrounded && input != null && input.JumpPressed)
            {
                _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
            }

            _verticalVelocity += _gravity * Time.deltaTime;
            Vector3 finalVelocity = move * speed + Vector3.up * _verticalVelocity;
            _cc.Move(finalVelocity * Time.deltaTime);
        }

        public void SetForcedCrouch(bool value)
        {
            _forcedCrouch = value;
            RefreshImmediateStance();
        }

        /// <summary>Temporary crouch ownership used by slide/evasion without interfering with hiding spots.</summary>
        public void SetEvasionCrouch(bool value)
        {
            _evasionCrouch = value;
            RefreshImmediateStance();
        }

        /// <summary>Returns false while world geometry blocks the full standing capsule.</summary>
        public bool CanStand()
        {
            if (_cc == null || !_cc.enabled) return true;
            float radius = Mathf.Max(.01f, _cc.radius - .015f);
            float halfSegment = Mathf.Max(0f, (_standingHeight * .5f) - radius);
            Vector3 center = transform.TransformPoint(new Vector3(0f, _standingHeight * .5f, 0f));
            Vector3 up = transform.up;
            int count = Physics.OverlapCapsuleNonAlloc(center - up * halfSegment, center + up * halfSegment,
                radius, _stanceOverlaps, ~0, QueryTriggerInteraction.Ignore);
            for (int index = 0; index < count; index++)
            {
                Collider candidate = _stanceOverlaps[index];
                if (candidate != null && !candidate.transform.IsChildOf(transform)) return false;
            }
            return true;
        }

        private void RefreshImmediateStance()
        {
            EscapeRoomRevolt.Core.Input.InputRouter input = EscapeRoomRevolt.Core.Input.InputRouter.Instance;
            bool wantsCrouch = _forcedCrouch || _evasionCrouch || (input != null && input.CrouchHeld);
            if (!wantsCrouch && _isCrouching && !CanStand()) wantsCrouch = true;
            _isCrouching = wantsCrouch;
            ApplyStance(_isCrouching ? _crouchHeight : _standingHeight, true);
        }

        private void ApplyStance(float targetHeight, bool immediate)
        {
            _cc.height = immediate
                ? targetHeight
                : Mathf.Lerp(_cc.height, targetHeight, Time.deltaTime * _crouchTransitionSpeed);
            _cc.center = new Vector3(0f, _cc.height / 2f, 0f);
            if (_playerCamera == null) return;
            float heightRatio = _cc.height / _standingHeight;
            Vector3 targetCamPos = _originalCameraLocalPosition;
            targetCamPos.y = _originalCameraLocalPosition.y * heightRatio;
            _playerCamera.localPosition = immediate
                ? targetCamPos
                : Vector3.Lerp(_playerCamera.localPosition, targetCamPos, Time.deltaTime * _crouchTransitionSpeed);
        }

        private void PlayFootstepSound()
        {
            if (_surfaceAudioData == null || EscapeRoomRevolt.Systems.Audio.AudioManager.Instance == null) return;

            string currentTag = "Untagged";

            // Raycast down to find floor material
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2.5f))
            {
                currentTag = hit.collider.tag;
            }

            AudioClip clip = _surfaceAudioData.GetRandomClip(currentTag);
            if (clip != null)
            {
                // Play sound via AudioManager
                EscapeRoomRevolt.Systems.Audio.AudioManager.Instance.PlaySoundAt(clip, transform.position, 1f, 0.15f);
            }
        }

        // ── Public API ───────────────────────────────────────────────────────
        /// <summary>Overrides the mouse sensitivity at runtime (e.g. from settings menu).</summary>
        public void SetMouseSensitivity(float sensitivity) => _mouseSensitivity = sensitivity;

        // ── Save / Load ──────────────────────────────────────────────────────
        public string SaveId => "Player";

        [System.Serializable]
        private class PlayerSaveState
        {
            public Vector3 position;
            public Quaternion rotation;
            public float cameraPitch;
        }

        public string SaveData()
        {
            var state = new PlayerSaveState
            {
                position = transform.position,
                rotation = transform.rotation,
                cameraPitch = _cameraPitch
            };
            return JsonUtility.ToJson(state);
        }

        public void LoadData(string json)
        {
            var state = JsonUtility.FromJson<PlayerSaveState>(json);
            if (state == null) return;

            if (_cc != null) _cc.enabled = false;
            
            transform.position = state.position;
            transform.rotation = state.rotation;
            
            if (_cc != null) _cc.enabled = true;

            _cameraPitch = state.cameraPitch;
            _evasionCrouch = false;
            if (_playerCamera != null)
            {
                _playerCamera.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
            }
        }
    }
}
