using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace EscapeRoomRevolt.Player.VR
{
    /// <summary>
    /// Bridges an XR controller's ray to the UI Toolkit panel rendered onto this GameObject's
    /// quad — pointer move, click (select) and scroll. Reuses the same XRBaseInteractable
    /// hover/select pipeline VRInteractionBridge already relies on for gameplay objects to know
    /// WHEN a controller is pointing here, then does its own Collider.Raycast against the quad's
    /// MeshCollider (for RaycastHit.textureCoord) instead of going through Unity's
    /// EventSystem/PanelRaycaster path or the panel's own picking — both confirmed broken for a
    /// WorldSpace-rendered UIDocument in this Unity version. See VRUIToolkitPresenter, which
    /// renders the panel to a texture on this same quad instead of using WorldSpace render mode.
    /// </summary>
    [RequireComponent(typeof(MeshCollider))]
    public sealed class VRUIPointerBridge : MonoBehaviour
    {
        private UIDocument _document;
        private MeshCollider _collider;
        private XRSimpleInteractable _interactable;
        private readonly Dictionary<IXRHoverInteractor, int> _pointerIds = new Dictionary<IXRHoverInteractor, int>();
        private int _nextPointerId = PointerId.trackedPointerIdBase;

        /// <summary>The UIDocument whose panel this bridge should drive. Set by VRUIToolkitPresenter.</summary>
        public void Configure(UIDocument document) => _document = document;

        private void Awake()
        {
            _collider = GetComponent<MeshCollider>();
            _interactable = GetComponent<XRSimpleInteractable>();
            if (_interactable == null) _interactable = gameObject.AddComponent<XRSimpleInteractable>();
        }

        private void OnEnable()
        {
            if (_interactable == null) return;
            _interactable.hoverEntered.AddListener(HandleHoverEntered);
            _interactable.hoverExited.AddListener(HandleHoverExited);
            _interactable.selectEntered.AddListener(HandleSelectEntered);
            _interactable.selectExited.AddListener(HandleSelectExited);
        }

        private void OnDisable()
        {
            if (_interactable == null) return;
            _interactable.hoverEntered.RemoveListener(HandleHoverEntered);
            _interactable.hoverExited.RemoveListener(HandleHoverExited);
            _interactable.selectEntered.RemoveListener(HandleSelectEntered);
            _interactable.selectExited.RemoveListener(HandleSelectExited);
            _pointerIds.Clear();
        }

        private void Update()
        {
            if (_pointerIds.Count == 0 || _document == null) return;
            IPanel panel = _document.rootVisualElement?.panel;
            if (panel == null) return;

            foreach (KeyValuePair<IXRHoverInteractor, int> entry in _pointerIds)
            {
                if (!TryRaycastPanel(entry.Key, out Vector2 panelPos)) continue;
                DispatchMouseEvent<PointerMoveEvent>(panel, panelPos, EventType.MouseMove);
                DispatchScrollIfAny(entry.Key, panel, panelPos);
            }
        }

        private void HandleHoverEntered(HoverEnterEventArgs args)
        {
            if (args.interactorObject is not IXRHoverInteractor interactor) return;
            _pointerIds[interactor] = _nextPointerId++;
        }

        private void HandleHoverExited(HoverExitEventArgs args)
        {
            if (args.interactorObject is not IXRHoverInteractor interactor) return;
            if (!_pointerIds.Remove(interactor)) return;

            IPanel panel = _document?.rootVisualElement?.panel;
            if (panel != null) DispatchMouseEvent<PointerLeaveEvent>(panel, Vector2.zero, EventType.MouseLeaveWindow);
        }

        private void HandleSelectEntered(SelectEnterEventArgs args) => DispatchButtonEvent<PointerDownEvent>(args.interactorObject as IXRHoverInteractor, EventType.MouseDown);
        private void HandleSelectExited(SelectExitEventArgs args) => DispatchButtonEvent<PointerUpEvent>(args.interactorObject as IXRHoverInteractor, EventType.MouseUp);

        private void DispatchButtonEvent<T>(IXRHoverInteractor interactor, EventType eventType) where T : PointerEventBase<T>, new()
        {
            if (interactor == null || !_pointerIds.ContainsKey(interactor)) return;
            IPanel panel = _document?.rootVisualElement?.panel;
            if (panel == null || !TryRaycastPanel(interactor, out Vector2 panelPos)) return;
            DispatchMouseEvent<T>(panel, panelPos, eventType);
        }

        /// <summary>
        /// Casts from the interactor's own ray origin against this quad's MeshCollider to read
        /// RaycastHit.textureCoord, then scales it into the panel's own live layout size — not the
        /// render texture's raw pixel size, which does not necessarily match it (confirmed by
        /// direct testing: the panel's internal coordinate space is independent of the texture's
        /// resolution).
        /// </summary>
        private bool TryRaycastPanel(IXRHoverInteractor interactor, out Vector2 panelPos)
        {
            panelPos = default;
            if (_collider == null || !_collider.enabled || _document == null || _document.rootVisualElement == null)
                return false;
            Transform origin = ResolveRayOrigin(interactor);
            if (origin == null) return false;
            if (!_collider.Raycast(new Ray(origin.position, origin.forward), out RaycastHit hit, 100f)) return false;

            Rect layout = _document.rootVisualElement.layout;
            if (float.IsNaN(layout.width) || float.IsNaN(layout.height)) return false;

            panelPos = new Vector2(hit.textureCoord.x * layout.width, (1f - hit.textureCoord.y) * layout.height);
            return true;
        }

        private static Transform ResolveRayOrigin(IXRHoverInteractor interactor)
        {
            if (interactor is NearFarInteractor nearFar) return nearFar.curveOrigin;
            if (interactor is IXRRayProvider rayProvider) return rayProvider.GetOrCreateRayOrigin();
            return null;
        }

        private static void DispatchMouseEvent<T>(IPanel panel, Vector2 panelPosition, EventType eventType) where T : PointerEventBase<T>, new()
        {
            Event systemEvent = new Event { mousePosition = panelPosition, type = eventType, button = 0, clickCount = 1 };
            using T evt = PointerEventBase<T>.GetPooled(systemEvent);
            panel.visualTree.SendEvent(evt);
        }

        private static void DispatchScrollIfAny(IXRHoverInteractor interactor, IPanel panel, Vector2 panelPosition)
        {
            if (interactor is not NearFarInteractor nearFar) return;
            if (!nearFar.uiScrollInput.TryReadValue(out Vector2 scroll) || scroll.sqrMagnitude < .0001f) return;

            Event systemEvent = new Event { mousePosition = panelPosition, type = EventType.ScrollWheel, delta = new Vector2(-scroll.x, -scroll.y) };
            using WheelEvent evt = WheelEvent.GetPooled(systemEvent);
            panel.visualTree.SendEvent(evt);
        }
    }
}
