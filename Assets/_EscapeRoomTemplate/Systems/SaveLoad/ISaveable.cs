namespace EscapeRoomRevolt.Systems.SaveLoad
{
    /// <summary>
    /// Interface for any object that needs to save and load its state.
    /// All ISaveable objects in the scene are found and processed by the SaveManager.
    /// </summary>
    public interface ISaveable
    {
        /// <summary>
        /// A unique, stable ID across game sessions.
        /// (e.g. "MainDoor", "SafePuzzle", "Inventory_Item_Key")
        /// </summary>
        string SaveId { get; }

        /// <summary>
        /// Called when the game is saving. Return a JSON string or serializable object
        /// representing the current state.
        /// </summary>
        object SaveState();

        /// <summary>
        /// Called when the game is loading. The state object is passed back in.
        /// </summary>
        void LoadState(object state);
    }
}
