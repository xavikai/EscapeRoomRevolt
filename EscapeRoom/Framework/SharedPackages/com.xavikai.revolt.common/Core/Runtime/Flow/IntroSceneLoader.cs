using UnityEngine;

namespace EscapeRoomRevolt.Core.Flow
{
    /// <summary>
    /// Hands control to the main menu once the intro finishes. Kept as its own component so the
    /// intro's CutsceneSequence stays a plain "play this and tell me when you're done" and knows
    /// nothing about scene flow.
    /// </summary>
    public sealed class IntroSceneLoader : MonoBehaviour
    {
        public void LoadMainMenu() => GameFlowManager.EnsureInstance().ReturnToMainMenu();
    }
}
