namespace EscapeRoomRevolt.Core
{
    /// <summary>
    /// Cached, EventBus-driven mirror of whether gameplay input should be blocked because a UI
    /// panel is open. Systems/Player read this instead of reaching into UIManager/
    /// UIToolkitMenuController singletons directly — GameplayUIController and
    /// UIToolkitMenuController publish <see cref="OnGameplayUIBlockingChanged"/> and
    /// <see cref="OnMenuUIBlockingChanged"/> whenever their own state flips, and this class keeps
    /// the last known value of each.
    ///
    /// Resubscribed by GameContext right after every EventBus.Clear(), since this is a static
    /// subscriber with no scene lifecycle (OnEnable/OnDisable) to rely on for that.
    /// </summary>
    public static class GameplayBlockState
    {
        private static bool _gameplayModalBlocking;
        private static bool _menuScreenBlocking;
        private static bool _subscribed;

        /// <summary>True while a GameplayUIController modal (inventory, note, keypad, examiner) is open.</summary>
        public static bool IsGameplayModalBlocking => _gameplayModalBlocking;

        /// <summary>True while a UIToolkitMenuController screen (main, pause, settings...) is open.</summary>
        public static bool IsMenuScreenBlocking => _menuScreenBlocking;

        /// <summary>True while either source is blocking gameplay input.</summary>
        public static bool IsBlocking => _gameplayModalBlocking || _menuScreenBlocking;

        /// <summary>Clears the cached flags and (re)subscribes to the source events. Call after EventBus.Clear().</summary>
        public static void Reset()
        {
            _gameplayModalBlocking = false;
            _menuScreenBlocking = false;
            _subscribed = false;
            EnsureSubscribed();
        }

        private static void EnsureSubscribed()
        {
            if (_subscribed) return;
            _subscribed = true;
            EventBus.Subscribe<OnGameplayUIBlockingChanged>(HandleGameplayModalChanged);
            EventBus.Subscribe<OnMenuUIBlockingChanged>(HandleMenuScreenChanged);
        }

        private static void HandleGameplayModalChanged(OnGameplayUIBlockingChanged evt) => _gameplayModalBlocking = evt.isBlocking;
        private static void HandleMenuScreenChanged(OnMenuUIBlockingChanged evt) => _menuScreenBlocking = evt.isBlocking;
    }
}
