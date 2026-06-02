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
        [Header("Key Bindings")]
        [SerializeField] private KeyCode _inventoryKey = KeyCode.I;
        [SerializeField] private KeyCode _pauseKey     = KeyCode.Escape;

        private void Update()
        {
            if (UIManager.Instance == null) return;

            if (Input.GetKeyDown(_inventoryKey))
                UIManager.Instance.ToggleInventory();

            if (Input.GetKeyDown(_pauseKey))
                HandlePause();
        }

        // ── Private Methods ──────────────────────────────────────────────────
        private void HandlePause()
        {
            // If a secondary UI panel is open (note reader, puzzle), close it first
            if (UIManager.Instance.IsUIBlockingGameplay)
            {
                UIManager.Instance.CloseTopPanel();
                return;
            }

            // Otherwise toggle the pause menu
            UIManager.Instance.TogglePauseMenu();
        }
    }
}
