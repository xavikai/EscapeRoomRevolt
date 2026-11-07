using System;
using System.Collections.Generic;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Inventory
{
    /// <summary>Controls how much assistance the inventory gives when an interactable requires an item.</summary>
    public enum ItemUsePolicy
    {
        SelectedOnly,
        OfferCompatible,
        AutoUseSingle
    }

    public enum ItemUseResult { Rejected, NoCompatibleItem, OfferedSelection, Used }

    public interface IInventoryItemTarget
    {
        ItemUsePolicy UsePolicy { get; }
        bool ConsumeItemOnUse { get; }
        bool AcceptsItem(InventoryItemData item);
        bool TryUseItem(InventoryItemData item);
    }

    public sealed class InventoryItemUseRequest
    {
        private readonly InventoryManager _inventory;
        private readonly IInventoryItemTarget _target;

        public IReadOnlyList<InventoryItemData> Candidates { get; }
        public bool IsResolved { get; private set; }

        internal InventoryItemUseRequest(
            InventoryManager inventory,
            IInventoryItemTarget target,
            IReadOnlyList<InventoryItemData> candidates)
        {
            _inventory = inventory;
            _target = target;
            Candidates = candidates;
        }

        public bool IsCompatible(string itemId)
        {
            foreach (InventoryItemData candidate in Candidates)
                if (candidate != null && string.Equals(candidate.ItemId, itemId, StringComparison.Ordinal)) return true;
            return false;
        }

        public bool TryUse(string itemId)
        {
            if (IsResolved || !IsCompatible(itemId)) return false;
            if (_target is UnityEngine.Object unityObject && unityObject == null) return false;
            IsResolved = _inventory.TryApplyItem(_target, itemId);
            return IsResolved;
        }
    }
}
