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

        [Header("Debug")]
        [SerializeField] private bool _showDebugRay = true;

        private IInteractable _currentTarget;
        private Camera _camera;

        // ── Events ───────────────────────────────────────────────────────────
        /// <summary>Called every frame when the focused interactable changes.</summary>
        public event System.Action<IInteractable> OnFocusChanged;

        // ── Unity Lifecycle ──────────────────────────────────────────────────
        private void Awake()
        {
            _camera = GetComponent<Camera>();
            if (_camera == null)
                _camera = Camera.main;
        }

        private void Update()
        {
            if (EscapeRoomRevolt.UI.PC.UIManager.Instance != null && EscapeRoomRevolt.UI.PC.UIManager.Instance.IsUIBlockingGameplay)
                return;

            DetectInteractable();

            if (_currentTarget != null && _currentTarget.CanInteract && Input.GetKeyDown(_interactKey))
                TriggerInteraction();
        }

        // ── Private Methods ──────────────────────────────────────────────────
        private void DetectInteractable()
        {
            Ray ray = new Ray(_camera.transform.position, _camera.transform.forward);

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
    }
}
