using System;
using System.Collections.Generic;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Puzzle
{
    [Serializable]
    public sealed class PlacementRule
    {
        [Tooltip("Id of the piece (matches whatever calls Connect(pieceId, ...)).")]
        public string pieceId = "piece";
        [Tooltip("Id of the socket this piece must reach to solve the puzzle.")]
        public string correctSocketId = "socket";
    }

    /// <summary>
    /// Place-each-piece-in-its-socket puzzle: each piece in the rule list must end up in its correct
    /// socket, in any order. Scene interactables (a grabbed piece, a UI drag, anything) call
    /// Connect(pieceId, socketId) when a piece is placed and Disconnect(pieceId) when removed. A
    /// socket only ever holds one piece, matching how a physical socket behaves — the piece
    /// previously there (if any) is removed automatically.
    /// </summary>
    public class PlacementPuzzle : PuzzleController
    {
        [Header("Placement")]
        [SerializeField] private List<PlacementRule> _rules = new List<PlacementRule>();
        [Tooltip("Automatically checks the solution once every piece has some placement.")]
        [SerializeField] private bool _autoCheckWhenFull = true;
        [Tooltip("Scrambles which socket each piece must reach each playthrough (same pieces and sockets, seeded from SaveManager.RunSeed), instead of always requiring the authored pairing.")]
        [SerializeField] private bool _randomizeMapping;

        private readonly Dictionary<string, string> _connections = new Dictionary<string, string>();

        public int PieceCount => _rules.Count;
        public int PlacedCount => _connections.Count;

        protected override void Awake()
        {
            base.Awake();
            if (_randomizeMapping) ShuffleMapping(new System.Random(ResolveVariantSeed()));
        }

        /// <summary>Permutes the correctSocketId values among the existing rules — same pieces, same sockets, different (still one-to-one) pairing.</summary>
        private void ShuffleMapping(System.Random random)
        {
            List<string> sockets = new List<string>(_rules.Count);
            foreach (PlacementRule rule in _rules) sockets.Add(rule.correctSocketId);

            for (int i = sockets.Count - 1; i > 0; i--)
            {
                int swapIndex = random.Next(i + 1);
                (sockets[i], sockets[swapIndex]) = (sockets[swapIndex], sockets[i]);
            }

            for (int i = 0; i < _rules.Count; i++) _rules[i].correctSocketId = sockets[i];
        }

        /// <summary>Places a piece into a socket, removing whichever piece (if any) already occupied that socket.</summary>
        public void Connect(string pieceId, string socketId)
        {
            if (IsSolved || string.IsNullOrEmpty(pieceId) || string.IsNullOrEmpty(socketId)) return;
            SetInProgress();

            string previousPieceAtSocket = null;
            foreach (KeyValuePair<string, string> entry in _connections)
            {
                if (entry.Value == socketId && entry.Key != pieceId) { previousPieceAtSocket = entry.Key; break; }
            }
            if (previousPieceAtSocket != null) _connections.Remove(previousPieceAtSocket);

            _connections[pieceId] = socketId;

            if (_autoCheckWhenFull && _connections.Count >= _rules.Count) SubmitConnections();
        }

        /// <summary>Removes a piece without affecting the others. Wrong placements stay in place otherwise, so the player can fix them one at a time.</summary>
        public void Disconnect(string pieceId)
        {
            if (IsSolved) return;
            _connections.Remove(pieceId);
        }

        public string GetConnectedSocket(string pieceId) => _connections.TryGetValue(pieceId, out string socketId) ? socketId : null;

        /// <summary>Which piece (if any) currently occupies a given socket — the inverse lookup of GetConnectedSocket, used by socket-side visuals.</summary>
        public string GetPieceAtSocket(string socketId)
        {
            foreach (KeyValuePair<string, string> entry in _connections)
                if (entry.Value == socketId) return entry.Key;
            return null;
        }

        /// <summary>Checks the current placements against the solution. Safe to call manually even with auto-check off.</summary>
        public void SubmitConnections()
        {
            if (IsSolved) return;

            foreach (PlacementRule rule in _rules)
            {
                if (!_connections.TryGetValue(rule.pieceId, out string socketId) || socketId != rule.correctSocketId)
                {
                    Fail("Wrong placement");
                    return;
                }
            }
            Solve();
        }

        protected override void OnPuzzleReset() => _connections.Clear();

        [Serializable]
        private sealed class ConnectionEntry { public string pieceId; public string socketId; }

        [Serializable]
        private sealed class PlacementPuzzleSaveData
        {
            public int stateIndex;
            public List<ConnectionEntry> connections = new List<ConnectionEntry>();
            public List<ConnectionEntry> chosenMapping = new List<ConnectionEntry>();
        }

        public override string SaveData()
        {
            PlacementPuzzleSaveData data = new PlacementPuzzleSaveData { stateIndex = (int)State };
            foreach (KeyValuePair<string, string> entry in _connections)
                data.connections.Add(new ConnectionEntry { pieceId = entry.Key, socketId = entry.Value });
            foreach (PlacementRule rule in _rules)
                data.chosenMapping.Add(new ConnectionEntry { pieceId = rule.pieceId, socketId = rule.correctSocketId });
            return JsonUtility.ToJson(data);
        }

        public override void LoadData(string json)
        {
            base.LoadData(json);
            PlacementPuzzleSaveData data = JsonUtility.FromJson<PlacementPuzzleSaveData>(json);
            _connections.Clear();
            if (data == null) return;

            if (data.chosenMapping != null)
            {
                foreach (ConnectionEntry entry in data.chosenMapping)
                {
                    PlacementRule rule = _rules.Find(r => r.pieceId == entry.pieceId);
                    if (rule != null) rule.correctSocketId = entry.socketId;
                }
            }

            if (data.connections == null) return;
            foreach (ConnectionEntry entry in data.connections) _connections[entry.pieceId] = entry.socketId;
        }
    }
}
