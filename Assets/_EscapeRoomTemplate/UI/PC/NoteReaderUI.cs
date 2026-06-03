using UnityEngine;
using TMPro;
using EscapeRoomRevolt.Core;

namespace EscapeRoomRevolt.UI.PC
{
    /// <summary>
    /// Displays the content of a ReadableNote on the screen.
    /// Listens for the OnNoteRead event from the EventBus.
    /// </summary>
    public class NoteReaderUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _noteTextDisplay;
        
        public void DisplayText(string content)
        {
            if (_noteTextDisplay != null)
            {
                _noteTextDisplay.text = content;
            }
        }

        /// <summary>
        /// Called by a UI "Close" button.
        /// </summary>
        public void CloseNote()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.HideNoteReader();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
