using EscapeRoomRevolt.UI.PC;
using EscapeRoomRevolt.UI.Toolkit;
using UnityEngine;

namespace EscapeRoomRevolt.Player.VR
{
    /// <summary>Keeps XR ray colliders active only while their UI document accepts input.</summary>
    [RequireComponent(typeof(BoxCollider))]
    public sealed class VRUIPanelColliderController : MonoBehaviour
    {
        public enum PanelKind { Gameplay, Menu }
        [SerializeField] private PanelKind _panelKind;
        private BoxCollider _collider;

        public void Configure(PanelKind kind) => _panelKind = kind;
        private void Awake() => _collider = GetComponent<BoxCollider>();

        private void LateUpdate()
        {
            if (_collider == null) return;
            _collider.enabled = _panelKind == PanelKind.Menu
                ? UIToolkitMenuController.Instance != null && UIToolkitMenuController.Instance.IsBlockingGameplay
                : UIManager.Instance != null && UIManager.Instance.IsUIBlockingGameplay;
        }
    }
}
