using TMPro;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Flow
{
    /// <summary>Simple world-space or UI countdown view for TimedGameOverHazard.</summary>
    public sealed class HazardCountdownDisplay : MonoBehaviour
    {
        [SerializeField] private TimedGameOverHazard _hazard;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private string _readyText = "READY";

        private void Awake()
        {
            if (_label == null) _label = GetComponent<TMP_Text>();
        }

        private void Update()
        {
            if (_label == null || _hazard == null) return;
            if (_hazard.HasFailed) _label.text = "GAME OVER";
            else if (!_hazard.IsRunning) _label.text = _readyText;
            else
            {
                int seconds = Mathf.CeilToInt(_hazard.TimeRemaining);
                _label.text = $"{seconds / 60:00}:{seconds % 60:00}";
            }
        }
    }
}
