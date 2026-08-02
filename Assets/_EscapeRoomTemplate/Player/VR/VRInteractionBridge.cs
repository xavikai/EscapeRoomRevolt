using EscapeRoomRevolt.Systems.Interaction;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace EscapeRoomRevolt.Player.VR
{
    /// <summary>
    /// Event-driven bridge between XRI and the framework's platform-neutral interactions.
    /// Existing gameplay interactables therefore keep the same implementation on PC and VR.
    /// </summary>
    public sealed class VRInteractionBridge : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour _interactableSource;
        [SerializeField] private PlayerHand _hand = PlayerHand.Right;
        [SerializeField] private bool _interactOnSelect = true;
        private IInteractable _interactable;
        private XRBaseInteractable _xrInteractable;

        private void Awake()
        {
            Resolve();
            _xrInteractable = GetComponent<XRBaseInteractable>();
        }

        private void OnEnable()
        {
            if (_xrInteractable == null) _xrInteractable = GetComponent<XRBaseInteractable>();
            if (_xrInteractable == null) return;
            _xrInteractable.hoverEntered.AddListener(HandleHoverEntered);
            _xrInteractable.hoverExited.AddListener(HandleHoverExited);
            _xrInteractable.selectEntered.AddListener(HandleSelectEntered);
        }

        private void OnDisable()
        {
            if (_xrInteractable == null) return;
            _xrInteractable.hoverEntered.RemoveListener(HandleHoverEntered);
            _xrInteractable.hoverExited.RemoveListener(HandleHoverExited);
            _xrInteractable.selectEntered.RemoveListener(HandleSelectEntered);
        }

        public void BeginFocus()
        {
            Resolve();
            if (_interactable.IsAlive()) _interactable.OnFocusEnter();
        }

        public void EndFocus()
        {
            Resolve();
            if (_interactable.IsAlive()) _interactable.OnFocusExit();
        }

        public void Select()
        {
            Resolve();
            InteractionDispatcher.TryPerform(_interactable, _hand);
        }

        private void Resolve()
        {
            if (_interactableSource is IInteractable configured)
            {
                _interactable = configured;
                return;
            }

            foreach (MonoBehaviour behaviour in GetComponentsInParent<MonoBehaviour>(true))
            {
                if (behaviour is not IInteractable interactable) continue;
                _interactableSource = behaviour;
                _interactable = interactable;
                return;
            }
        }

        private void HandleHoverEntered(HoverEnterEventArgs _) => BeginFocus();
        private void HandleHoverExited(HoverExitEventArgs _) => EndFocus();
        private void HandleSelectEntered(SelectEnterEventArgs _) { if (_interactOnSelect) Select(); }
    }
}
