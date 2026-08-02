using UnityEngine;
using EscapeRoomRevolt.UI.PC;

namespace EscapeRoomRevolt.Player.PC
{
    /// <summary>
    /// Handles global player input that is NOT movement:
    ///   - [I]   Toggle Inventory
    ///   - [Esc] Pause / Unpause
    ///
    /// Keep this separate from PlayerMovement so responsibilities are clear.
    /// Attach to the same root Player GameObject.
    /// </summary>
    public class PlayerInputHandler : MonoBehaviour
    {
        private void Update()
        {
            if (UIManager.Instance == null) return;

            var input = EscapeRoomRevolt.Core.Input.InputRouter.Instance;
            if (input == null) return;

            if (input.InventoryPressed)
                UIManager.Instance.ToggleInventory();

            if (input.PausePressed)
                HandlePause();

            EscapeRoomRevolt.Systems.Inventory.InventoryManager inventory = EscapeRoomRevolt.Systems.Inventory.InventoryManager.Instance;
            if (inventory != null && !UIManager.Instance.IsUIBlockingGameplay)
            {
                if (input.TryGetQuickSlotPressed(out int slot)) inventory.SetActiveQuickSlot(slot);
                if (input.QuickNavigatePerformed && Mathf.Abs(input.QuickNavigate) > .01f)
                    inventory.NavigateQuickAccess(input.QuickNavigate > 0f ? -1 : 1);
            }
        }

        // ── Private Methods ──────────────────────────────────────────────────
        private void HandlePause()
        {
            // Gameplay overlays own Escape before the pause document does.
            if (UIManager.Instance.IsUIBlockingGameplay)
            {
                UIManager.Instance.CloseTopPanel();
                return;
            }

            if (EscapeRoomRevolt.UI.Toolkit.UIToolkitMenuController.Instance != null)
            {
                EscapeRoomRevolt.UI.Toolkit.UIToolkitMenuController.Instance.TogglePause();
                return;
            }

            // Otherwise toggle the pause menu
            UIManager.Instance.TogglePauseMenu();
        }
    }
}
