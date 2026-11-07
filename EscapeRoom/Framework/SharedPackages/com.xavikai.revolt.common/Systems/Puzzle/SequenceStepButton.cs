using UnityEngine;

namespace EscapeRoomRevolt.Systems.Puzzle
{
    /// <summary>Lets a plain UnityEvent (e.g. InteractableTrigger.OnInteractEvent) feed a step into SequencePuzzle from the Inspector, without the designer having to pass the id in code.</summary>
    public sealed class SequenceStepButton : MonoBehaviour
    {
        [SerializeField] private SequencePuzzle _puzzle;
        [SerializeField] private string _stepId;

        public void Press()
        {
            if (_puzzle != null) _puzzle.InputStep(_stepId);
        }
    }
}
