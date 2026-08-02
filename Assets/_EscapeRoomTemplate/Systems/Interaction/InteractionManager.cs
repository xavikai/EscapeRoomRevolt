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
    [RequireComponent(typeof(Camera))]
    public class InteractionManager : MonoBehaviour
    {
        [Header("Raycast Settings")]
        [SerializeField] private float _interactionRange = 2.5f;
        [SerializeField] private LayerMask _interactableLayer;

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

            if (GetComponent<PhysicsGrabber>() == null)
            {
                gameObject.AddComponent<PhysicsGrabber>();
            }
        }

        private void Update()
        {
            if (!_currentTarget.IsAlive() && _currentTarget != null)
                SwitchFocus(null);

            if ((EscapeRoomRevolt.UI.PC.UIManager.Instance != null && EscapeRoomRevolt.UI.PC.UIManager.Instance.IsUIBlockingGameplay)
                || (EscapeRoomRevolt.UI.Toolkit.UIToolkitMenuController.Instance != null
                    && EscapeRoomRevolt.UI.Toolkit.UIToolkitMenuController.Instance.IsBlockingGameplay))
                return;

            // Block InteractionManager if we are currently holding a physics object
            if (PhysicsGrabber.Instance != null && PhysicsGrabber.Instance.IsHoldingObject)
            {
                if (_currentTarget.IsAlive()) SwitchFocus(null);
                return;
            }

            DetectInteractable();

            // Interaction remains available regardless of cursor lock state. UI panels
            // that must consume input are already filtered by IsUIBlockingGameplay above.
            if (_currentTarget.IsAlive() && _currentTarget.CanInteract)
            {
                bool routedInput = EscapeRoomRevolt.Core.Input.InputRouter.Instance != null
                    && EscapeRoomRevolt.Core.Input.InputRouter.Instance.InteractPressed;
                if (routedInput)
                    TriggerInteraction();
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

            if (!detected.IsAlive()) detected = null;

            if (detected != _currentTarget)
                SwitchFocus(detected);
        }

        private void SwitchFocus(IInteractable newTarget)
        {
            if (_currentTarget.IsAlive()) _currentTarget.OnFocusExit();
            _currentTarget = newTarget.IsAlive() ? newTarget : null;
            if (_currentTarget.IsAlive()) _currentTarget.OnFocusEnter();
            OnFocusChanged?.Invoke(_currentTarget);

            if (EscapeRoomRevolt.UI.PC.UIManager.Instance != null)
            {
                if (_currentTarget.IsAlive() && _currentTarget.CanInteract)
                {
                    EscapeRoomRevolt.UI.PC.UIManager.Instance.SetCrosshair(_currentTarget.InteractionCursor);
                }
                else
                {
                    EscapeRoomRevolt.UI.PC.UIManager.Instance.SetCrosshair(CursorType.Default);
                }
            }
        }

        private void TriggerInteraction()
        {
            IInteractable target = _currentTarget;
            if (!target.IsAlive())
            {
                SwitchFocus(null);
                return;
            }

            InteractionDispatcher.TryPerform(target);

            // Pickups commonly disable or destroy themselves. Releasing focus here avoids
            // retaining a dead interface reference until the next raycast frame.
            SwitchFocus(null);
        }

        // ── Public API ───────────────────────────────────────────────────────
        /// <summary>The object currently in the player's crosshair (can be null).</summary>
        public IInteractable CurrentTarget => _currentTarget.IsAlive() ? _currentTarget : null;

        /// <summary>Whether the player is currently looking at something interactable.</summary>
        public bool HasTarget => _currentTarget.IsAlive() && _currentTarget.CanInteract;

        /// <summary>Forces the interaction manager to raycast from a specific camera (useful for puzzle zoom).</summary>
        public void SetOverrideCamera(Camera cam) => _overrideCamera = cam;

        /// <summary>Restores the interaction manager to use the player's main camera.</summary>
        public void ClearOverrideCamera() => _overrideCamera = null;
    }
}
