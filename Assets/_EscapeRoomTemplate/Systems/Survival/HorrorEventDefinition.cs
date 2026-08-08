using UnityEngine;

namespace EscapeRoomRevolt.Systems.Survival
{
    [CreateAssetMenu(fileName = "HorrorEvent", menuName = "Escape Room Framework/Survival/Horror Event", order = 31)]
    public sealed class HorrorEventDefinition : ScriptableObject
    {
        [SerializeField] private string _persistentId = "horror_event_unique_id";
        [SerializeField] private string _displayName = "Evento ambiental";
        [TextArea(2, 5)] [SerializeField] private string _subtitle;
        [Tooltip("Seconds the subtitle stays on screen once it has finished typing, then it hides itself.")]
        [Min(0f)] [SerializeField] private float _subtitleSeconds = 4f;
        [SerializeField] private AudioClip _audio;
        [Range(0f, 1f)] [SerializeField] private float _maximumSanity = 1f;
        [Min(0f)] [SerializeField] private float _stressApplied = 8f;
        [SerializeField] private bool _onlyOnce = true;
        [Min(0f)] [SerializeField] private float _cooldown = 20f;

        public string PersistentId => _persistentId;
        public string DisplayName => _displayName;
        public string Subtitle => _subtitle;
        public float SubtitleSeconds => _subtitleSeconds;
        public AudioClip Audio => _audio;
        public float MaximumSanity => _maximumSanity;
        public float StressApplied => _stressApplied;
        public bool OnlyOnce => _onlyOnce;
        public float Cooldown => _cooldown;
    }
}
