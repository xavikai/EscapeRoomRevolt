using UnityEngine;
using EscapeRoomRevolt.Core;

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
            var input = EscapeRoomRevolt.Core.Input.InputRouter.Instance;
            if (input == null) return;

            if (input.InventoryPressed)
                EventBus.Publish(new RequestToggleInventory());

            if (input.PausePressed)
                HandlePause();

            if (!GameplayBlockState.IsGameplayModalBlocking)
            {
                if (input.TryGetQuickSlotPressed(out int slot))
                    EventBus.Publish(new RequestSetActiveQuickSlot { slot = slot });
                if (input.QuickNavigatePerformed && Mathf.Abs(input.QuickNavigate) > .01f)
                    EventBus.Publish(new RequestNavigateQuickAccess { direction = input.QuickNavigate > 0f ? -1 : 1 });
            }
        }

        // ── Private Methods ──────────────────────────────────────────────────
        private void HandlePause()
        {
            // Gameplay overlays own Escape before the pause document does.
            if (GameplayBlockState.IsGameplayModalBlocking)
            {
                EventBus.Publish(new RequestCloseTopPanel());
                return;
            }

            EventBus.Publish(new RequestTogglePause());
        }
    }
}
