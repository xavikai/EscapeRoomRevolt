using System.Collections.Generic;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Puzzle
{
    /// <summary>
    /// A puzzle that requires interacting with objects in a specific order.
    /// E.g. pulling levers, pressing colored buttons, or stepping on plates.
    /// </summary>
    public class SequencePuzzle : PuzzleController
    {
        [Header("Sequence Settings")]
        [Tooltip("The correct sequence of IDs that the player must input")]
        [SerializeField] private List<string> _correctSequence;
        [Tooltip("Shuffles the order of the steps above each playthrough (same steps, seeded from SaveManager.RunSeed), instead of always requiring the authored order.")]
        [SerializeField] private bool _randomizeOrder;

        private List<string> _currentSequence = new List<string>();

        protected override void Awake()
        {
            base.Awake();
            if (_randomizeOrder) ShuffleSequence(new System.Random(ResolveVariantSeed()));
        }

        private void ShuffleSequence(System.Random random)
        {
            for (int i = _correctSequence.Count - 1; i > 0; i--)
            {
                int swapIndex = random.Next(i + 1);
                (_correctSequence[i], _correctSequence[swapIndex]) = (_correctSequence[swapIndex], _correctSequence[i]);
            }
        }

        /// <summary>Registers an input step into the current sequence.</summary>
        public void InputStep(string stepId)
        {
            if (IsSolved) return;

            SetInProgress();
            _currentSequence.Add(stepId);

            CheckSequence();
        }

        private void CheckSequence()
        {
            // Check if the current steps match the beginning of the correct sequence
            for (int i = 0; i < _currentSequence.Count; i++)
            {
                if (_currentSequence[i] != _correctSequence[i])
                {
                    Fail("Wrong sequence step");
                    _currentSequence.Clear();
                    return;
                }
            }

            // If we reached the end of the sequence successfully
            if (_currentSequence.Count == _correctSequence.Count)
            {
                Solve();
            }
        }

        protected override void OnPuzzleReset() => _currentSequence.Clear();

        [System.Serializable]
        private sealed class SequenceSaveData
        {
            public int stateIndex;
            public List<string> chosenOrder;
        }

        public override string SaveData()
        {
            return JsonUtility.ToJson(new SequenceSaveData { stateIndex = (int)State, chosenOrder = _correctSequence });
        }

        public override void LoadData(string json)
        {
            base.LoadData(json);
            SequenceSaveData data = JsonUtility.FromJson<SequenceSaveData>(json);
            if (data?.chosenOrder != null && data.chosenOrder.Count > 0) _correctSequence = data.chosenOrder;
        }
    }
}
