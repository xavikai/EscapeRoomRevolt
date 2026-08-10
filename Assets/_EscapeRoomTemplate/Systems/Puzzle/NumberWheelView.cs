using TMPro;
using UnityEngine;
using EscapeRoomRevolt.Systems.Interaction;

namespace EscapeRoomRevolt.Systems.Puzzle
{
    /// <summary>
    /// Presentation adapter for a SteppedPositioner used as a decimal combination wheel. The puzzle
    /// logic remains StatePuzzle, so wheels, levers and dials share the same tested state machinery.
    /// </summary>
    [RequireComponent(typeof(SteppedPositioner))]
    public sealed class NumberWheelView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _digitLabel;
        [SerializeField] private int _minimumDigit;

        private SteppedPositioner _positioner;

        public int CurrentDigit => _minimumDigit + Positioner.CurrentIndex;

        private SteppedPositioner Positioner
        {
            get
            {
                if (_positioner == null) _positioner = GetComponent<SteppedPositioner>();
                return _positioner;
            }
        }

        private void Awake()
        {
            _positioner = GetComponent<SteppedPositioner>();
            if (_digitLabel == null) _digitLabel = GetComponentInChildren<TMP_Text>(true);
        }

        private void OnEnable()
        {
            Positioner.OnPositionChanged?.AddListener(OnPositionChanged);
            Refresh();
        }

        private void OnDisable()
        {
            if (_positioner != null) _positioner.OnPositionChanged?.RemoveListener(OnPositionChanged);
        }

        private void OnPositionChanged(int _) => Refresh();

        public void Refresh()
        {
            if (_digitLabel != null) _digitLabel.text = CurrentDigit.ToString();
        }
    }
}
