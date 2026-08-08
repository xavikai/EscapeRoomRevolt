using UnityEngine;

namespace EscapeRoomRevolt.Systems.Equipment
{
    /// <summary>
    /// Hides an equippable's own mesh while it is held, and shows it again once dropped.
    ///
    /// A first-person item with no hands attached to it reads as a prop floating in the corner of
    /// the screen: it costs a lot of view and tells the player nothing. Hiding the mesh keeps
    /// whatever the item *does* — a flashlight's beam, for instance — while giving the view back.
    /// Only Renderers are switched, never the GameObject, so lights and colliders keep working.
    /// </summary>
    [RequireComponent(typeof(EquippableItem))]
    public sealed class EquippedModelVisibility : MonoBehaviour
    {
        [Tooltip("Hide the item's own mesh while it is equipped. Turn this off once real first-person hands hold it.")]
        [SerializeField] private bool _hideWhileEquipped = true;

        private EquippableItem _item;
        private Renderer[] _renderers;
        private bool _hidden;

        private void Awake()
        {
            _item = GetComponent<EquippableItem>();
            _renderers = GetComponentsInChildren<Renderer>(true);
        }

        private void LateUpdate()
        {
            // Polled rather than event-driven: EquippableItem exposes no equip/unequip event, and
            // this is a handful of bool comparisons on one object.
            bool shouldHide = _hideWhileEquipped && IsEquipped();
            if (shouldHide == _hidden) return;

            _hidden = shouldHide;
            foreach (Renderer renderer in _renderers)
                if (renderer != null) renderer.enabled = !shouldHide;
        }

        /// <summary>An equipped item is parented to the player's equipment socket, so its parent is the tell.</summary>
        private bool IsEquipped()
        {
            EquipmentController controller = EquipmentController.Instance;
            return controller != null && controller.CurrentItem == _item;
        }
    }
}
