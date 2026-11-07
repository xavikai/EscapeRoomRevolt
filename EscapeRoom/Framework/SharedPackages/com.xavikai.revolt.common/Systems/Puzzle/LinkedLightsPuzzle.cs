using System;
using UnityEngine;
using UnityEngine.Events;

namespace EscapeRoomRevolt.Systems.Puzzle
{
    [Serializable]
    public sealed class LightCircuitNode
    {
        public bool initiallyOn;
        public bool targetOn = true;
        [Tooltip("Indices toggled by this button, including itself if desired. Duplicates are ignored.")]
        public int[] connections = Array.Empty<int>();
        public Renderer indicator;
    }

    /// <summary>A configurable Lights Out circuit. Saves partial progress and supports reset.</summary>
    public sealed class LinkedLightsPuzzle : PuzzleController
    {
        [SerializeField] private LightCircuitNode[] _nodes = Array.Empty<LightCircuitNode>();
        [SerializeField] private Color _onColor = new Color(.2f, 1f, .65f);
        [SerializeField] private Color _offColor = new Color(.07f, .1f, .14f);
        [SerializeField] private UnityEvent _onChanged = new UnityEvent();
        private bool[] _lights;
        private MaterialPropertyBlock _properties;
        public int Count => _nodes.Length;
        public bool IsLightOn(int index) => _lights != null && index >= 0 && index < _lights.Length && _lights[index];

        protected override void Awake()
        {
            ResetLights();
            base.Awake();
        }

        public void Press(int index)
        {
            if (IsSolved || index < 0 || index >= _nodes.Length || _nodes[index] == null) return;
            if (_lights == null) ResetLights();
            int[] links = _nodes[index].connections;
            if (links == null || links.Length == 0) return;
            SetInProgress();
            for (int i = 0; i < links.Length; i++)
            {
                int target = links[i];
                if (target < 0 || target >= _lights.Length || Array.IndexOf(links, target) != i) continue;
                _lights[target] = !_lights[target];
            }
            RefreshLights();
            _onChanged.Invoke();
            for (int i = 0; i < _nodes.Length; i++)
                if (_nodes[i] == null || _lights[i] != _nodes[i].targetOn) return;
            if (_nodes.Length > 0) Solve();
        }

        private void ResetLights()
        {
            _lights = new bool[_nodes.Length];
            for (int i = 0; i < _lights.Length; i++) _lights[i] = _nodes[i] != null && _nodes[i].initiallyOn;
            RefreshLights();
        }

        private void RefreshLights()
        {
            if (_lights == null) return;
            if (_properties == null) _properties = new MaterialPropertyBlock();
            for (int i = 0; i < _nodes.Length; i++)
            {
                Renderer indicator = _nodes[i]?.indicator;
                if (indicator == null) continue;
                indicator.GetPropertyBlock(_properties);
                _properties.SetColor("_BaseColor", _lights[i] ? _onColor : _offColor);
                _properties.SetColor("_Color", _lights[i] ? _onColor : _offColor);
                indicator.SetPropertyBlock(_properties);
            }
        }

        protected override void OnPuzzleReset() => ResetLights();
        [Serializable] private class CircuitSaveData { public int stateIndex; public bool[] lights; }
        public override string SaveData() => JsonUtility.ToJson(new CircuitSaveData { stateIndex = (int)State, lights = _lights });
        public override void LoadData(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            CircuitSaveData data = JsonUtility.FromJson<CircuitSaveData>(json);
            if (data == null || data.lights == null || data.lights.Length != _nodes.Length) return;
            _lights = (bool[])data.lights.Clone();
            base.LoadData(json);
            RefreshLights();
        }
    }
}
