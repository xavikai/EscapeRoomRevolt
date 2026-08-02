using UnityEngine;

namespace EscapeRoomRevolt.Core.Flow
{
    /// <summary>Project-level scene routing used by the framework's game flow.</summary>
    [CreateAssetMenu(fileName = "GameFlowSettings", menuName = "Escape Room Framework/Game Flow Settings")]
    public sealed class GameFlowSettings : ScriptableObject
    {
        [SerializeField] private string _mainMenuScene = "MainMenu";
        [SerializeField] private string _firstGameplayScene = "ShowcaseMuseum";
        [Tooltip("Optional preferred Continue slot. Leave empty to load the most recently saved slot.")]
        [SerializeField] private string _continueSlot = "";

        public string MainMenuScene => _mainMenuScene;
        public string FirstGameplayScene => _firstGameplayScene;
        public string ContinueSlot => _continueSlot;
    }
}
