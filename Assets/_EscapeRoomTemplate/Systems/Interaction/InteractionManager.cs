using UnityEngine;
using EscapeRoomRevolt.Core;

namespace EscapeRoomRevolt.Systems.Interaction
{
    /// <summary>
    /// PC first-person interaction system.
    /// Attach to the Player's Camera. Casts a ray every frame and detects
    /// IInteractable objects within range. Press the interact key to trigger them.
    ///
    /// Publishes: OnInteractionPerformed
    /// </summary>
    public class InteractionManager : MonoBehaviour
    {
        [Header("Raycast Settings")]
        [SerializeField] private float _interactionRange = 2.5f;
        [SerializeField] private LayerMask _interactableLayer;
        [SerializeField] private KeyCode _interactKey = KeyCode.E;

        [Header("Global Settings")]
        [Tooltip("Material used for outlining objects when hovered. Assign your Shader Graph outline here once, and all interactables will use it.")]
        [SerializeField] private Material _globalOutlineMaterial;

        [Header("Debug")]
        [SerializeField] private bool _showDebugRay = true;

        private IInteractable _currentTarget;
        private Camera _mainCamera;
        private Camera _overrideCamera;

        public static InteractionManager Instance { get; private set; }
        public Material GlobalOutlineMaterial => _globalOutlineMaterial;

        // ── Events ───────────────────────────────────────────────────────────
        /// <summary>Called every frame when the focused interactable changes.</summary>
        public event System.Action<IInteractable> OnFocusChanged;

        // ── Unity Lifecycle ──────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            _mainCamera = GetComponent<Camera>();
            if (_mainCamera == null)
                _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (EscapeRoomRevolt.UI.PC.UIManager.Instance != null && EscapeRoomRevolt.UI.PC.UIManager.Instance.IsUIBlockingGameplay)
                return;

            DetectInteractable();

            // Support either 'E' key or Left Mouse Click for interaction
            if (_currentTarget != null && _currentTarget.CanInteract)
            {
                bool isMouseFree = Cursor.lockState == CursorLockMode.None || Cursor.visible;

                if (isMouseFree)
                {
                    // In Focus Mode, only allow mouse clicks
                    if (Input.GetMouseButtonDown(0))
                    {
                        TriggerInteraction();
                    }
                }
                else
                {
                    // In FPS Mode, allow Interaction Key or mouse clicks
                    if (Input.GetKeyDown(_interactKey) || Input.GetMouseButtonDown(0))
                    {
                        TriggerInteraction();
                    }
                }
            }
        }

        // ── Private Methods ──────────────────────────────────────────────────
        private void DetectInteractable()
        {
            Camera activeCamera = _overrideCamera != null ? _overrideCamera : _mainCamera;
            if (activeCamera == null) return;

            Ray ray;
            if (Cursor.lockState == CursorLockMode.None || Cursor.visible)
            {
                // Mouse is free, raycast from pointer
                ray = activeCamera.ScreenPointToRay(Input.mousePosition);
            }
            else
            {
                // FPS style, raycast from center of screen
                ray = new Ray(activeCamera.transform.position, activeCamera.transform.forward);
            }

            if (_showDebugRay)
                Debug.DrawRay(ray.origin, ray.direction * _interactionRange, Color.cyan);

            IInteractable detected = null;

            if (Physics.Raycast(ray, out RaycastHit hit, _interactionRange, _interactableLayer))
                detected = hit.collider.GetComponentInParent<IInteractable>();

            if (detected != _currentTarget)
                SwitchFocus(detected);
        }

        private void SwitchFocus(IInteractable newTarget)
        {
            _currentTarget?.OnFocusExit();
            _currentTarget = newTarget;
            _currentTarget?.OnFocusEnter();
            OnFocusChanged?.Invoke(_currentTarget);
        }

        private void TriggerInteraction()
        {
            _currentTarget.Interact();

            EventBus.Publish(new OnInteractionPerformed
            {
                interactableId = (_currentTarget as MonoBehaviour)?.name ?? "Unknown",
                target = (_currentTarget as MonoBehaviour)?.gameObject
            });
        }

        // ── Public API ───────────────────────────────────────────────────────
        /// <summary>The object currently in the player's crosshair (can be null).</summary>
        public IInteractable CurrentTarget => _currentTarget;

        /// <summary>Whether the player is currently looking at something interactable.</summary>
        public bool HasTarget => _currentTarget != null && _currentTarget.CanInteract;

        /// <summary>Forces the interaction manager to raycast from a specific camera (useful for puzzle zoom).</summary>
        public void SetOverrideCamera(Camera cam) => _overrideCamera = cam;

        /// <summary>Restores the interaction manager to use the player's main camera.</summary>
        public void ClearOverrideCamera() => _overrideCamera = null;
    }
}
