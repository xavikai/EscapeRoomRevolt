using System;
using UnityEngine;

namespace EscapeRoomRevolt.UI.PC
{
    [Obsolete("Interaction prompts are rendered by GameplayUIController (UI Toolkit).")]
    public sealed class InteractionPromptUI : MonoBehaviour
    {
        private void OnEnable() => enabled = false;
    }
}
