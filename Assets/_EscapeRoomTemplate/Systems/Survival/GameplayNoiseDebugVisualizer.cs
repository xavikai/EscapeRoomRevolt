using System.Collections.Generic;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Survival
{
    /// <summary>Optional scene gizmo that displays recent AI-hearing stimuli.</summary>
    public sealed class GameplayNoiseDebugVisualizer : MonoBehaviour
    {
        private struct Entry
        {
            public GameplayNoiseStimulus stimulus;
            public float expiresAt;
        }

        [SerializeField] private bool _showGizmos = true;
        [SerializeField, Min(.1f)] private float _displaySeconds = 2f;
        [SerializeField, Range(1, 128)] private int _maximumEntries = 32;
        private readonly List<Entry> _entries = new List<Entry>(32);

        private void OnEnable() => GameplayNoise.Emitted += HandleNoise;
        private void OnDisable() => GameplayNoise.Emitted -= HandleNoise;

        private void Update()
        {
            for (int index = _entries.Count - 1; index >= 0; index--)
                if (Time.time >= _entries[index].expiresAt) _entries.RemoveAt(index);
        }

        private void HandleNoise(GameplayNoiseStimulus stimulus)
        {
            if (_entries.Count >= _maximumEntries) _entries.RemoveAt(0);
            _entries.Add(new Entry { stimulus = stimulus, expiresAt = Time.time + _displaySeconds });
        }

        private void OnDrawGizmos()
        {
            if (!_showGizmos || !Application.isPlaying) return;
            foreach (Entry entry in _entries)
            {
                float life = Mathf.Clamp01((entry.expiresAt - Time.time) / Mathf.Max(.01f, _displaySeconds));
                Gizmos.color = GetColor(entry.stimulus.Type, life);
                Gizmos.DrawWireSphere(entry.stimulus.Position, entry.stimulus.Radius);
                Gizmos.DrawSphere(entry.stimulus.Position, .08f);
            }
        }

        private static Color GetColor(GameplayNoiseType type, float alpha)
        {
            Color color = type switch
            {
                GameplayNoiseType.Footstep => Color.cyan,
                GameplayNoiseType.Sprint => new Color(1f, .5f, 0f),
                GameplayNoiseType.Door => Color.yellow,
                GameplayNoiseType.DoorCareful => new Color(.45f, .8f, .35f),
                GameplayNoiseType.DoorSlam => new Color(1f, .2f, .05f),
                GameplayNoiseType.Impact => Color.red,
                _ => Color.magenta
            };
            color.a = alpha;
            return color;
        }
    }
}
