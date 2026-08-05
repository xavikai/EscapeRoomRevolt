using UnityEngine;

namespace EscapeRoomRevolt.Systems.Puzzle
{
    /// <summary>Void-returning wrapper so a plain UnityEvent can drive SlidingPuzzle.TryMoveTile from the Inspector — its bool return breaks direct UnityEvent persistent-listener binding.</summary>
    public sealed class SlidingTileButton : MonoBehaviour
    {
        [SerializeField] private SlidingPuzzle _puzzle;
        [SerializeField] private string _tileId;
        public void Move() => _puzzle.TryMoveTile(_tileId);
    }
}
