using UnityEngine;
using EscapeRoomRevolt.UI.PC;

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
    public class PlayerMovement : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────
        [Header("Movement")]
        [SerializeField] private float _walkSpeed    = 3.5f;
        [SerializeField] private float _sprintSpeed  = 6.0f;
        [SerializeField] private float _gravity      = -9.81f;

        [Header("Mouse Look")]
        [SerializeField] private Transform _playerCamera;
        [SerializeField] private float _mouseSensitivity = 120f;
        [SerializeField] private float _maxLookAngle     = 85f;

        // ── Private State ────────────────────────────────────────────────────
        private CharacterController _cc;
        private float  _verticalVelocity;
        private float  _cameraPitch; // Up/down rotation accumulated

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
            // Horizontal movement
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            bool isSprinting = Input.GetKey(KeyCode.LeftShift);
            float speed = isSprinting ? _sprintSpeed : _walkSpeed;

            Vector3 move = transform.right * h + transform.forward * v;
            move = Vector3.ClampMagnitude(move, 1f); // Prevents diagonal speed boost

            // Gravity
            if (_cc.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f; // Small negative to keep grounded

            _verticalVelocity += _gravity * Time.deltaTime;
            move.y = _verticalVelocity;

            _cc.Move(move * speed * Time.deltaTime);
        }

        // ── Public API ───────────────────────────────────────────────────────
        /// <summary>Overrides the mouse sensitivity at runtime (e.g. from settings menu).</summary>
        public void SetMouseSensitivity(float sensitivity) => _mouseSensitivity = sensitivity;
    }
}
