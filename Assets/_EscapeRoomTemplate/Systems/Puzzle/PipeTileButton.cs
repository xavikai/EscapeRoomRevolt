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
}
