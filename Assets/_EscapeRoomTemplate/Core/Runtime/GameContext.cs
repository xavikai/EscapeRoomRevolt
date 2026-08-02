using UnityEngine;

namespace EscapeRoomRevolt.Core
{
    /// <summary>
    /// Global read-only access point for core game systems.
    /// Populated by the Bootstrapper at scene start.
    /// 
    /// Usage:
    ///   var inventory = GameContext.Inventory;
    ///   var saveManager = GameContext.SaveManager;
    /// </summary>
    public static class GameContext
    {
        // ── Core Systems ────────────────────────────────
        // These will be set by the Bootstrapper on scene load.
        // Systems will be added here as they are implemented (EPIC 03, 05, etc.)

        public static bool IsInitialized { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            IsInitialized = false;
            EventBus.Clear();
        }

        /// <summary>
        /// Called by the Bootstrapper once all systems are ready.
        /// </summary>
        internal static void MarkInitialized()
        {
            IsInitialized = true;
            Debug.Log("[GameContext] All systems initialized.");
        }

        /// <summary>
        /// Resets context on scene unload / game restart.
        /// </summary>
        internal static void ResetForNewSession()
        {
            IsInitialized = false;
            EventBus.Clear();
            Debug.Log("[GameContext] New session state cleared.");
        }
    }
}
