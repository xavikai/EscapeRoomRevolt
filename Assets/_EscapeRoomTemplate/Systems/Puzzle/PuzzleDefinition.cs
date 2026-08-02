using EscapeRoomRevolt.Systems.Hint;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Puzzle
{
    public enum PuzzleCategory { Code, Sequence, State, Socket, Observation, Custom }

    /// <summary>Reusable authoring data shared by puzzle prefabs without coupling scene logic.</summary>
    [CreateAssetMenu(fileName = "PuzzleDefinition", menuName = "Escape Room Framework/Puzzles/Puzzle Definition", order = 20)]
    public sealed class PuzzleDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _persistentId = "puzzle_unique_id";
        [SerializeField] private string _displayName = "Nuevo puzle";
        [SerializeField] private PuzzleCategory _category = PuzzleCategory.Custom;

        [Header("Player Guidance")]
        [TextArea(2, 4)] [SerializeField] private string _objective = "Encuentra una solución.";
        [SerializeField] private HintData _hints;

        [Header("Failure Consequences")]
        [Min(0f)] [SerializeField] private float _sanityPenalty = 4f;

        public string PersistentId => _persistentId;
        public string DisplayName => _displayName;
        public PuzzleCategory Category => _category;
        public string Objective => _objective;
        public HintData Hints => _hints;
        public float SanityPenalty => _sanityPenalty;

        private void OnValidate()
        {
            _persistentId = (_persistentId ?? string.Empty).Trim().ToLowerInvariant().Replace(' ', '_');
            _sanityPenalty = Mathf.Max(0f, _sanityPenalty);
        }
    }
}
