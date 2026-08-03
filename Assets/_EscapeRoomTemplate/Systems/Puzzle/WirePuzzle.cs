using System;
using System.Collections.Generic;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Puzzle
{
    [Serializable]
    public sealed class WireConnectionRule
    {
        [Tooltip("Id of the wire endpoint (matches whatever calls Connect(wireId, ...)).")]
        public string wireId = "wire";
        [Tooltip("Id of the socket this wire must reach to solve the puzzle.")]
        public string correctSocketId = "socket";
    }

    /// <summary>
    /// Connect-the-cables puzzle: each wire in the rule list must end up plugged into its correct
    /// socket, in any order. Scene interactables (a grabbed wire endpoint, a UI drag, anything)
    /// call Connect(wireId, socketId) when a wire is plugged in and Disconnect(wireId) when
    /// unplugged. A socket only ever holds one wire, matching how a physical socket behaves — the
    /// wire previously there (if any) is unplugged automatically.
    /// </summary>
    public class WirePuzzle : PuzzleController
    {
        [Header("Wiring")]
        [SerializeField] private List<WireConnectionRule> _rules = new List<WireConnectionRule>();
        [Tooltip("Automatically checks the solution once every wire has some connection.")]
        [SerializeField] private bool _autoCheckWhenFull = true;
        [Tooltip("Scrambles which socket each wire must reach each playthrough (same wires and sockets, seeded from SaveManager.RunSeed), instead of always requiring the authored pairing.")]
        [SerializeField] private bool _randomizeMapping;

        private readonly Dictionary<string, string> _connections = new Dictionary<string, string>();

        public int WireCount => _rules.Count;
        public int ConnectedCount => _connections.Count;

        protected override void Awake()
        {
            base.Awake();
            if (_randomizeMapping) ShuffleMapping(new System.Random(ResolveVariantSeed()));
        }

        /// <summary>Permutes the correctSocketId values among the existing rules — same wires, same sockets, different (still one-to-one) pairing.</summary>
        private void ShuffleMapping(System.Random random)
        {
            List<string> sockets = new List<string>(_rules.Count);
            foreach (WireConnectionRule rule in _rules) sockets.Add(rule.correctSocketId);

            for (int i = sockets.Count - 1; i > 0; i--)
            {
                int swapIndex = random.Next(i + 1);
                (sockets[i], sockets[swapIndex]) = (sockets[swapIndex], sockets[i]);
            }

            for (int i = 0; i < _rules.Count; i++) _rules[i].correctSocketId = sockets[i];
        }

        /// <summary>Plugs a wire into a socket, unplugging whichever wire (if any) already occupied that socket.</summary>
        public void Connect(string wireId, string socketId)
        {
            if (IsSolved || string.IsNullOrEmpty(wireId) || string.IsNullOrEmpty(socketId)) return;
            SetInProgress();

            string previousWireAtSocket = null;
            foreach (KeyValuePair<string, string> entry in _connections)
            {
                if (entry.Value == socketId && entry.Key != wireId) { previousWireAtSocket = entry.Key; break; }
            }
            if (previousWireAtSocket != null) _connections.Remove(previousWireAtSocket);

            _connections[wireId] = socketId;

            if (_autoCheckWhenFull && _connections.Count >= _rules.Count) SubmitConnections();
        }

        /// <summary>Unplugs a wire without affecting the others. Wrong connections stay in place otherwise, so the player can fix them one at a time.</summary>
        public void Disconnect(string wireId)
        {
            if (IsSolved) return;
            _connections.Remove(wireId);
        }

        public string GetConnectedSocket(string wireId) => _connections.TryGetValue(wireId, out string socketId) ? socketId : null;

        /// <summary>Checks the current connections against the solution. Safe to call manually even with auto-check off.</summary>
        public void SubmitConnections()
        {
            if (IsSolved) return;

            foreach (WireConnectionRule rule in _rules)
            {
                if (!_connections.TryGetValue(rule.wireId, out string socketId) || socketId != rule.correctSocketId)
                {
                    Fail("Wrong wiring");
                    return;
                }
            }
            Solve();
        }

        protected override void OnPuzzleReset() => _connections.Clear();

        [Serializable]
        private sealed class ConnectionEntry { public string wireId; public string socketId; }

        [Serializable]
        private sealed class WirePuzzleSaveData
        {
            public int stateIndex;
            public List<ConnectionEntry> connections = new List<ConnectionEntry>();
            public List<ConnectionEntry> chosenMapping = new List<ConnectionEntry>();
        }

        public override string SaveData()
        {
            WirePuzzleSaveData data = new WirePuzzleSaveData { stateIndex = (int)State };
            foreach (KeyValuePair<string, string> entry in _connections)
                data.connections.Add(new ConnectionEntry { wireId = entry.Key, socketId = entry.Value });
            foreach (WireConnectionRule rule in _rules)
                data.chosenMapping.Add(new ConnectionEntry { wireId = rule.wireId, socketId = rule.correctSocketId });
            return JsonUtility.ToJson(data);
        }

        public override void LoadData(string json)
        {
            base.LoadData(json);
            WirePuzzleSaveData data = JsonUtility.FromJson<WirePuzzleSaveData>(json);
            _connections.Clear();
            if (data == null) return;

            if (data.chosenMapping != null)
            {
                foreach (ConnectionEntry entry in data.chosenMapping)
                {
                    WireConnectionRule rule = _rules.Find(r => r.wireId == entry.wireId);
                    if (rule != null) rule.correctSocketId = entry.socketId;
                }
            }

            if (data.connections == null) return;
            foreach (ConnectionEntry entry in data.connections) _connections[entry.wireId] = entry.socketId;
        }
    }
}
