using UnityEngine;

namespace EscapeRoomRevolt.Systems.Puzzle
{
    /// <summary>Void-returning wrapper so a plain UnityEvent (e.g. InteractableTrigger.OnInteractEvent) can drive PipePuzzle.RotateTile from the Inspector — its bool return breaks direct UnityEvent persistent-listener binding.</summary>
    public sealed class PipeTileButton : MonoBehaviour
    {
        [SerializeField] private PipePuzzle _puzzle;
        [SerializeField] private string _tileId;
        public void Rotate() => _puzzle.RotateTile(_tileId);
    }

    /// <summary>Void-returning wrapper so a plain UnityEvent can drive SlidingPuzzle.TryMoveTile from the Inspector — its bool return breaks direct UnityEvent persistent-listener binding.</summary>
    public sealed class SlidingTileButton : MonoBehaviour
    {
        [SerializeField] private SlidingPuzzle _puzzle;
        [SerializeField] private string _tileId;
        public void Move() => _puzzle.TryMoveTile(_tileId);
    }

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
