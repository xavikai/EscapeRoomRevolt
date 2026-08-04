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
        [Header("Multi-room (optional)")]
        [Tooltip("Leave empty for a single-room game: completing this set ends the game with Completion Ending, exactly as before. Set this to send the player to another scene instead.")]
        [SerializeField] private string _nextRoomScene;
        [Tooltip("Matched against a RoomSpawnPoint's Spawn Id in the next room scene. Leave empty to use wherever that scene's Player prefab is already placed.")]
        [SerializeField] private string _nextRoomSpawnId;

        public string RoomId => string.IsNullOrWhiteSpace(_roomId) ? name : _roomId;
        public IReadOnlyList<ObjectiveDefinition> Objectives => _objectives;
        public EndingDefinition CompletionEnding => _completionEnding;
        public string NextRoomScene => _nextRoomScene;
        public string NextRoomSpawnId => _nextRoomSpawnId;
    }
}
