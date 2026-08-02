using System;
using EscapeRoomRevolt.UI.Toolkit;
using UnityEngine;

namespace EscapeRoomRevolt.UI.PC
{
    [Obsolete("Note rendering is owned by GameplayUIController (UI Toolkit).")]
    public sealed class NoteReaderUI : MonoBehaviour
    {
        public void DisplayText(string content) => GameplayUIController.Instance?.ShowNote(content);
        public void CloseNote() => GameplayUIController.Instance?.CloseTopPanel();
    }
}
