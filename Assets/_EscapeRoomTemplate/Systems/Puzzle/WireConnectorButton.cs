using UnityEngine;

namespace EscapeRoomRevolt.Systems.Puzzle
{
    /// <summary>Lets a plain UnityEvent drive WirePuzzle.Connect from the Inspector — it needs two string arguments, more than a single UnityEvent's static-argument binding supports.</summary>
    public sealed class WireConnectorButton : MonoBehaviour
    {
        [SerializeField] private WirePuzzle _puzzle;
        [SerializeField] private string _wireId;
        [SerializeField] private string _socketId;
        public void Connect() => _puzzle.Connect(_wireId, _socketId);
        public void Disconnect() => _puzzle.Disconnect(_wireId);
    }
}
