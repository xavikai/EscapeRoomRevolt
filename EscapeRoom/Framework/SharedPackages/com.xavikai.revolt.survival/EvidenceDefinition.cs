using UnityEngine;

namespace EscapeRoomRevolt.Systems.Survival
{
    [CreateAssetMenu(menuName = "Escape Room Framework/Survival/Evidence Definition", fileName = "Evidence")]
    public sealed class EvidenceDefinition : ScriptableObject
    {
        [SerializeField] private string _evidenceId = "evidence";
        [SerializeField] private string _title = "Evidence";
        [TextArea(2, 5)] [SerializeField] private string _description;
        [SerializeField, Min(.1f)] private float _recordingSeconds = 2.5f;
        [SerializeField, Min(.5f)] private float _maximumDistance = 12f;

        public string EvidenceId => string.IsNullOrWhiteSpace(_evidenceId) ? name : _evidenceId;
        public string Title => _title;
        public string Description => _description;
        public float RecordingSeconds => _recordingSeconds;
        public float MaximumDistance => _maximumDistance;
    }
}
