using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Puzzle
{
    /// <summary>
    /// Plays back the melody the player is meant to repeat, one note at a time.
    ///
    /// A sequence puzzle without a way to hear the answer is guesswork, not a puzzle: this is the
    /// clue. Point it at the same note buttons the player will press, so the demonstration and the
    /// answer can never drift apart — there is only one source of truth for which tone is which.
    /// </summary>
    public sealed class MelodyPlayer : MonoBehaviour
    {
        [Tooltip("The note buttons, in the order the melody should be played back.")]
        [SerializeField] private List<AudioSource> _notes = new List<AudioSource>();
        [Tooltip("Seconds between notes.")]
        [SerializeField, Min(.1f)] private float _gap = .6f;
        [Tooltip("Ignore repeat presses while it is still playing, so the melody can't overlap itself.")]
        [SerializeField] private bool _ignoreWhilePlaying = true;

        public bool IsPlaying { get; private set; }

        /// <summary>Plays the melody. Safe to call from a UnityEvent.</summary>
        public void Play()
        {
            if (_ignoreWhilePlaying && IsPlaying) return;
            StartCoroutine(PlayRoutine());
        }

        private IEnumerator PlayRoutine()
        {
            IsPlaying = true;
            foreach (AudioSource note in _notes)
            {
                if (note != null) note.Play();
                yield return new WaitForSeconds(_gap);
            }
            IsPlaying = false;
        }
    }
}
