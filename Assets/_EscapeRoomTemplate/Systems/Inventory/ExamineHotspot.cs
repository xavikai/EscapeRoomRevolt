using System;
using UnityEngine;
using UnityEngine.Events;

namespace EscapeRoomRevolt.Systems.Inventory
{
    /// <summary>
    /// Clickable point on an item's WorldPrefab, detected while the item is open in the 3D
    /// examiner (GameplayUIController). Add to a child GameObject with its own Collider — that
    /// collider is deliberately kept enabled even though the examiner disables every other
    /// collider on the model while examining, so it stays a valid raycast target.
    /// Reveal state persists across saves via ExamineHotspotRegistry, keyed by owning item + this
    /// hotspot's id, so re-examining an already-found hotspot shows its revealed text immediately.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class ExamineHotspot : MonoBehaviour
    {
        [SerializeField] private string _hotspotId = "hotspot";
        [TextArea(2, 3)]
        [SerializeField] private string _unrevealedPrompt = "Parece haber algo más aquí. Toca para examinarlo.";
        [TextArea(2, 4)]
        [SerializeField] private string _revealedDescription = "Has encontrado algo.";
        [Tooltip("If assigned, granted to the inventory the first time this hotspot is revealed.")]
        [SerializeField] private InventoryItemData _revealedItem;
        [SerializeField] private bool _onlyOnce = true;
        [SerializeField] private UnityEvent _onRevealed;

        public string UnrevealedPrompt => _unrevealedPrompt;
        public string RevealedDescription => _revealedDescription;

        public bool IsRevealed(string ownerItemId) =>
            ExamineHotspotRegistry.Instance != null && ExamineHotspotRegistry.Instance.IsRevealed(Key(ownerItemId));

        /// <summary>Marks this hotspot revealed for the given owning item, granting its item once. Safe to call repeatedly.</summary>
        public void Reveal(string ownerItemId)
        {
            string key = Key(ownerItemId);
            bool alreadyRevealed = ExamineHotspotRegistry.Instance != null && ExamineHotspotRegistry.Instance.IsRevealed(key);
            if (_onlyOnce && alreadyRevealed) return;

            ExamineHotspotRegistry.Instance?.MarkRevealed(key);
            if (!alreadyRevealed && _revealedItem != null) InventoryManager.Instance?.AddItem(_revealedItem);
            _onRevealed?.Invoke();
        }

        private string Key(string ownerItemId) => $"{ownerItemId}.{_hotspotId}";
    }
}
