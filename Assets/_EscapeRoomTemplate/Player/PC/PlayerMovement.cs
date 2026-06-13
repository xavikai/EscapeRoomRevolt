using UnityEngine;
using EscapeRoomRevolt.UI.PC;
using EscapeRoomRevolt.Core.Save;

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
        [SerializeField] private float _jumpHeight   = 1.2f;
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
        private Vector3 _originalCameraLocalPosition;

        // ── Unity Lifecycle ──────────────────────────────────────────────────
        private void Awake()
        {
            _cc = GetComponent<CharacterController>();

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
            bool uiBlocking = UIManager.Instance != null && UIManager.Instance.IsUIBlockingGameplay;
            if (uiBlocking) return;

            HandleMouseLook();
            HandleMovement();
        }

        // ── Private Methods ──────────────────────────────────────────────────
        private void HandleMouseLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity * Time.deltaTime;

            // Horizontal rotation — rotates the whole player body
            transform.Rotate(Vector3.up * mouseX);

            // Vertical rotation — only the camera, clamped to avoid flipping
            _cameraPitch -= mouseY;
            _cameraPitch  = Mathf.Clamp(_cameraPitch, -_maxLookAngle, _maxLookAngle);
            _playerCamera.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
        }

        private void HandleMovement()
        {
            // Crouching Input
            if (_canCrouch)
            {
                _isCrouching = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);
            }

            // Smoothly adjust CharacterController height and center
            float targetHeight = _isCrouching ? _crouchHeight : _standingHeight;
            _cc.height = Mathf.Lerp(_cc.height, targetHeight, Time.deltaTime * _crouchTransitionSpeed);
            _cc.center = new Vector3(0, _cc.height / 2f, 0);

            // Smoothly adjust Camera height based on current collider height proportion
            if (_playerCamera != null)
            {
                float heightRatio = _cc.height / _standingHeight;
                Vector3 targetCamPos = _originalCameraLocalPosition;
                targetCamPos.y = _originalCameraLocalPosition.y * heightRatio;
                _playerCamera.localPosition = Vector3.Lerp(_playerCamera.localPosition, targetCamPos, Time.deltaTime * _crouchTransitionSpeed);
            }

            // Horizontal movement
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            bool isSprinting = Input.GetKey(KeyCode.LeftShift) && !_isCrouching;
            float speed = _isCrouching ? _crouchSpeed : (isSprinting ? _sprintSpeed : _walkSpeed);

            Vector3 move = transform.right * h + transform.forward * v;
            move = Vector3.ClampMagnitude(move, 1f); // Prevents diagonal speed boost

            // Calculate horizontal distance moved this frame for footsteps
            Vector3 horizontalVelocity = move * speed;
            if (_cc.isGrounded && horizontalVelocity.sqrMagnitude > 0.1f)
            {
                _accumulatedDistance += horizontalVelocity.magnitude * Time.deltaTime;
                float currentStepThreshold = isSprinting ? _footstepDistanceSprint : _footstepDistanceWalk;

                if (_accumulatedDistance >= currentStepThreshold)
                {
                    _accumulatedDistance = 0f;
                    PlayFootstepSound();
                }
            }
            else
            {
                _accumulatedDistance = 0f; // Reset if stopped or jumping
            }

            // Gravity & Jumping
            if (_cc.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f; // Small negative to keep grounded

            if (_canJump && _cc.isGrounded && Input.GetButtonDown("Jump"))
            {
                _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
            }

            _verticalVelocity += _gravity * Time.deltaTime;
            move.y = _verticalVelocity;

            _cc.Move(move * speed * Time.deltaTime);
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
            if (_playerCamera != null)
            {
                _playerCamera.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
            }
        }
    }
}
