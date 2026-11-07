namespace EscapeRoomRevolt.Core.Save
{
    /// <summary>
    /// Implement this interface on any MonoBehaviour that needs to save and load state.
    /// The script must register itself with SaveManager.Instance on Awake/Start.
    /// </summary>
    public interface ISaveable
    {
        /// <summary>
        /// A unique identifier for this object. Use a persistent GUID or a unique name.
        /// </summary>
        string SaveId { get; }

        /// <summary>
        /// Serialize the specific state of this object into a JSON string.
        /// </summary>
        string SaveData();

        /// <summary>
        /// Restore the specific state of this object from a JSON string.
        /// </summary>
        void LoadData(string json);
    }
}
