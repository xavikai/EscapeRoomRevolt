using UnityEngine;
using EscapeRoomRevolt.Systems.Interaction;

namespace EscapeRoomRevolt.Systems.Puzzle
{
    /// <summary>Void-returning wrapper so a plain UnityEvent (e.g. InteractableTrigger.OnInteractEvent) can drive PipePuzzle.RotateTile from the Inspector — its bool return breaks direct UnityEvent persistent-listener binding.</summary>
    public sealed class PipeTileButton : MonoBehaviour
    {
        [SerializeField] private PipePuzzle _puzzle;
        [SerializeField] private string _tileId;
        [Tooltip("Optional: a SteppedPositioner that visually rotates the tile in sync with the puzzle's internal rotation state. Not required for the puzzle logic itself, which tracks rotation independently of how (or whether) it's rendered.")]
        [SerializeField] private SteppedPositioner _visualPositioner;

        private void Start()
        {
            // Sync the visual to whatever rotation the puzzle already has (randomized start, or a
            // loaded save) instead of waiting for the first click to snap it into place.
            if (_visualPositioner != null && _puzzle != null)
                _visualPositioner.SetIndexInstant(_puzzle.GetRotationSteps(_tileId));
        }

        public void Rotate()
        {
            _puzzle.RotateTile(_tileId);
            _visualPositioner?.Advance();
        }
    }
}
