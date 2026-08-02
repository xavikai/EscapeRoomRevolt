using System;
using EscapeRoomRevolt.Systems.Puzzle;
using EscapeRoomRevolt.UI.Toolkit;
using UnityEngine;

namespace EscapeRoomRevolt.UI.PC
{
    [Obsolete("Keypad rendering is owned by GameplayUIController (UI Toolkit).")]
    public sealed class KeypadUI : MonoBehaviour
    {
        public void Setup(CodePanelPuzzle puzzle) => GameplayUIController.Instance?.ShowKeypad(puzzle);
        public void OnDigitPressed(string digit) => GameplayUIController.Instance?.KeypadDigit(digit);
        public void OnClearPressed() => GameplayUIController.Instance?.KeypadClear();
        public void OnSubmitPressed() => GameplayUIController.Instance?.KeypadSubmit();
        public void Close() => GameplayUIController.Instance?.CloseTopPanel();
    }
}
