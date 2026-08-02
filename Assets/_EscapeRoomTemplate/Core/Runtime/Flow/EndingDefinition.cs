using UnityEngine;

namespace EscapeRoomRevolt.Core.Flow
{
    [CreateAssetMenu(fileName = "NewEnding", menuName = "Escape Room Framework/Ending Definition")]
    public sealed class EndingDefinition : ScriptableObject
    {
        [SerializeField] private string _endingId = "ending";
        [SerializeField] private GameOutcome _outcome = GameOutcome.Victory;
        [SerializeField] private string _title = "Has escapado";
        [TextArea(3, 8)] [SerializeField] private string _message = "La investigación ha terminado.";

        public string EndingId => string.IsNullOrWhiteSpace(_endingId) ? name : _endingId;
        public GameOutcome Outcome => _outcome;
        public string Title => _title;
        public string Message => _message;
    }
}
