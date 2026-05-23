using UnityEngine;

namespace EscapeRoomRevolt.Core
{
    /// <summary>
    /// Scene entry point. Place one instance of this MonoBehaviour
    /// in every room scene. It initializes all core systems in the
    /// correct order before gameplay begins.
    ///
    /// Execution order: -100 (runs before any other script)
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class Bootstrapper : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool _logInitialization = true;

        private void Awake()
        {
            Log("Bootstrapper starting...");
            InitializeSystems();
            GameContext.MarkInitialized();
            Log("Bootstrapper complete. All systems ready.");
        }

        private void InitializeSystems()
        {
            // Systems will be initialized here as they are implemented.
            // Order matters — add systems in dependency order:
            //
            // EPIC 03: InventoryManager
            // EPIC 04: PuzzleManager
            // EPIC 05: SaveManager
            // EPIC 06: UIManager
            //
            // Example (once implemented):
            //   GameContext.Inventory.Initialize();
            //   GameContext.SaveManager.Initialize();

            Log("Core systems initialized (EventBus ready).");
        }

        private void OnDestroy()
        {
            // Clean up when the scene unloads
            GameContext.Reset();
        }

        private void Log(string message)
        {
            if (_logInitialization)
                Debug.Log($"[Bootstrapper] {message}");
        }
    }
}
