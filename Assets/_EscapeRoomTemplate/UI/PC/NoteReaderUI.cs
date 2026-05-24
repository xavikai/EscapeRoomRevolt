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
        
        private void OnEnable()
        {
            EventBus.Subscribe<OnNoteRead>(HandleNoteRead);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<OnNoteRead>(HandleNoteRead);
        }

        private void HandleNoteRead(OnNoteRead data)
        {
            if (_noteTextDisplay != null)
            {
                _noteTextDisplay.text = data.content;
            }
            
            // Tell UIManager to show this panel (which also unlocks the cursor)
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowNoteReader();
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
