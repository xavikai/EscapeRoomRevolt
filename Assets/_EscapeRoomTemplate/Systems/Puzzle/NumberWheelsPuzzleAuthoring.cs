using System.Collections.Generic;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Puzzle
{
    /// <summary>
    /// Stores the designer-facing shape of a number-wheels puzzle. Runtime solving remains owned by
    /// StatePuzzle; this component only remembers how many decimal wheels the generated presentation
    /// should contain and which digit each one requires.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NumberWheelsPuzzleAuthoring : MonoBehaviour
    {
        public const int MinimumWheels = 2;
        public const int MaximumWheels = 8;

        [SerializeField, Range(MinimumWheels, MaximumWheels)] private int _wheelCount = 4;
        [SerializeField] private List<int> _combination = new List<int> { 3, 1, 4, 2 };

        public int WheelCount => _wheelCount;
        public IReadOnlyList<int> Combination => _combination;

        public int GetDigit(int index) => index >= 0 && index < _combination.Count
            ? _combination[index]
            : 0;

        public void SetCombination(IReadOnlyList<int> digits)
        {
            int requested = digits != null ? digits.Count : MinimumWheels;
            _wheelCount = Mathf.Clamp(requested, MinimumWheels, MaximumWheels);
            _combination.Clear();
            for (int index = 0; index < _wheelCount; index++)
            {
                int digit = digits != null && index < digits.Count ? digits[index] : 0;
                _combination.Add(Mathf.Clamp(digit, 0, 9));
            }
        }

        private void OnValidate()
        {
            _wheelCount = Mathf.Clamp(_wheelCount, MinimumWheels, MaximumWheels);
            if (_combination == null) _combination = new List<int>();
            while (_combination.Count < _wheelCount) _combination.Add(0);
            if (_combination.Count > _wheelCount)
                _combination.RemoveRange(_wheelCount, _combination.Count - _wheelCount);
            for (int index = 0; index < _combination.Count; index++)
                _combination[index] = Mathf.Clamp(_combination[index], 0, 9);
        }
    }
}
