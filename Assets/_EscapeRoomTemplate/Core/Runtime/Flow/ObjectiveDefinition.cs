using System.Collections.Generic;
using UnityEngine;

namespace EscapeRoomRevolt.Core.Flow
{
    public enum ObjectiveTrigger
    {
        Manual,
        PuzzleSolved,
        ItemCollected,
        NoteRead,
        InteractionPerformed,
        EvidenceRecorded
    }

    [CreateAssetMenu(fileName = "NewObjective", menuName = "Escape Room Framework/Objective Definition")]
    public sealed class ObjectiveDefinition : ScriptableObject
    {
        [SerializeField] private string _objectiveId = "objective";
        [SerializeField] private string _title = "New objective";
        [TextArea(2, 5)] [SerializeField] private string _description;
        [SerializeField] private bool _hiddenUntilAvailable;
        [SerializeField] private ObjectiveTrigger _trigger = ObjectiveTrigger.Manual;
        [Tooltip("Puzzle, item, note or interactable ID expected from the selected trigger.")]
        [SerializeField] private string _targetId;
        [Tooltip("All listed objectives must be complete before this one becomes available.")]
        [SerializeField] private List<ObjectiveDefinition> _prerequisites = new List<ObjectiveDefinition>();

        public string ObjectiveId => string.IsNullOrWhiteSpace(_objectiveId) ? name : _objectiveId;
        public string Title => _title;
        public string Description => _description;
        public bool HiddenUntilAvailable => _hiddenUntilAvailable;
        public ObjectiveTrigger Trigger => _trigger;
        public string TargetId => _targetId;
        public IReadOnlyList<ObjectiveDefinition> Prerequisites => _prerequisites;
    }
}
