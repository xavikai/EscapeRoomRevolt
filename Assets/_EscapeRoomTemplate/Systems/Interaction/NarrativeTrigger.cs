using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EscapeRoomRevolt.Core;
using EscapeRoomRevolt.Core.Save;
using EscapeRoomRevolt.UI.PC;

namespace EscapeRoomRevolt.Systems.Interaction
{
    public enum NarrativePlayMode
    {
        Once,
        Always,
        ProgressiveHints
    }

    [Serializable]
    public struct SubtitleLine
    {
        [TextArea(2, 4)]
        public string text;
        public float duration;
    }

    [Serializable]
    public class NarrativeSequence
    {
        [Tooltip("Optional audio clip to play with these subtitles")]
        public AudioClip audioClip;
        public List<SubtitleLine> subtitleLines = new List<SubtitleLine>();
    }

    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(AudioSource))]
    public class NarrativeTrigger : MonoBehaviour, ISaveable
    {
        [Header("Save System")]
        [SerializeField] private string _guid = System.Guid.NewGuid().ToString();

        [Header("Trigger Settings")]
        [SerializeField] private NarrativePlayMode _playMode = NarrativePlayMode.Once;
        [Tooltip("Seconds to wait before this trigger can be activated again (for Always and ProgressiveHints modes)")]
        [SerializeField] private float _cooldownTime = 5f;

        [Header("Narrative Content")]
        [Tooltip("For ProgressiveHints, the first element plays first. Next time triggered, the second plays, etc.")]
        [SerializeField] private List<NarrativeSequence> _sequences = new List<NarrativeSequence>();

        private AudioSource _audioSource;
        private Coroutine _subtitleCoroutine;
        
        // State variables
        private bool _hasPlayedOnce = false;
        private int _currentSequenceIndex = 0;
        private float _lastPlayTime = -9999f;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            GetComponent<BoxCollider>().isTrigger = true;
        }

        private void Start()
        {
            SaveManager.Instance?.Register(this);
        }

        private void OnDestroy()
        {
            SaveManager.Instance?.Unregister(this);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                TryPlay();
            }
        }

        private void TryPlay()
        {
            if (_sequences.Count == 0) return;
            
            if (_playMode == NarrativePlayMode.Once && _hasPlayedOnce)
                return;

            if (Time.time < _lastPlayTime + _cooldownTime)
                return;

            PlaySequence();
        }

        private void PlaySequence()
        {
            _hasPlayedOnce = true;
            _lastPlayTime = Time.time;

            NarrativeSequence seq = _sequences[_currentSequenceIndex];

            // Handle Audio
            if (seq.audioClip != null)
            {
                _audioSource.clip = seq.audioClip;
                _audioSource.Play();
            }

            // Handle Subtitles
            if (_subtitleCoroutine != null)
            {
                StopCoroutine(_subtitleCoroutine);
            }
            if (seq.subtitleLines.Count > 0)
            {
                _subtitleCoroutine = StartCoroutine(ShowSubtitlesRoutine(seq.subtitleLines));
            }

            // Advance index for ProgressiveHints
            if (_playMode == NarrativePlayMode.ProgressiveHints)
            {
                _currentSequenceIndex = Mathf.Min(_currentSequenceIndex + 1, _sequences.Count - 1);
            }
        }

        private IEnumerator ShowSubtitlesRoutine(List<SubtitleLine> lines)
        {
            if (UIManager.Instance == null) yield break;

            foreach (var line in lines)
            {
                UIManager.Instance.ShowSubtitle(line.text);
                yield return new WaitForSeconds(line.duration);
            }

            UIManager.Instance.HideSubtitle();
        }

        public string SaveId => _guid;

        public string SaveData()
        {
            var myState = new NarrativeState
            {
                hasPlayedOnce = _hasPlayedOnce,
                currentSequenceIndex = _currentSequenceIndex
            };
            return JsonUtility.ToJson(myState);
        }

        public void LoadData(string json)
        {
            if (!string.IsNullOrEmpty(json))
            {
                var myState = JsonUtility.FromJson<NarrativeState>(json);
                _hasPlayedOnce = myState.hasPlayedOnce;
                _currentSequenceIndex = myState.currentSequenceIndex;
            }
        }

        [Serializable]
        private class NarrativeState
        {
            public bool hasPlayedOnce;
            public int currentSequenceIndex;
        }

        private void OnDrawGizmos()
        {
            BoxCollider col = GetComponent<BoxCollider>();
            if (col != null)
            {
                Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
                Gizmos.DrawCube(col.center, col.size);
                Gizmos.color = new Color(0f, 1f, 0f, 1f);
                Gizmos.DrawWireCube(col.center, col.size);
            }
        }
    }
}
