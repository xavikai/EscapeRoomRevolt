using System.Collections.Generic;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Hint
{
    [System.Serializable]
    public class HintEntry
    {
        [TextArea(2, 5)]
        [Tooltip("The text to display as a subtitle.")]
        public string hintText;

        [Tooltip("Optional audio voiceover for the thought.")]
        public AudioClip hintAudio;
    }

    [CreateAssetMenu(fileName = "NewHintData", menuName = "EscapeRoom/Hint Data", order = 1)]
    public class HintData : ScriptableObject
    {
        [Tooltip("Time in seconds before the first hint is shown.")]
        public float delayBeforeFirstHint = 120f;

        [Tooltip("Time in seconds between subsequent hints.")]
        public float delayBetweenHints = 60f;

        [Tooltip("List of hints to display, ordered from least to most revealing.")]
        public List<HintEntry> hints = new List<HintEntry>();
    }
}
