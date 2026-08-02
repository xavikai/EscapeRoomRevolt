using System.Collections.Generic;
using UnityEngine;

namespace EscapeRoomRevolt.Core.Flow
{
    [CreateAssetMenu(fileName = "NewObjectiveSet", menuName = "Escape Room Framework/Objective Set")]
    public sealed class ObjectiveSet : ScriptableObject
    {
        [SerializeField] private string _roomId = "room";
        [SerializeField] private List<ObjectiveDefinition> _objectives = new List<ObjectiveDefinition>();
        [SerializeField] private EndingDefinition _completionEnding;

        public string RoomId => string.IsNullOrWhiteSpace(_roomId) ? name : _roomId;
        public IReadOnlyList<ObjectiveDefinition> Objectives => _objectives;
        public EndingDefinition CompletionEnding => _completionEnding;
    }
}
