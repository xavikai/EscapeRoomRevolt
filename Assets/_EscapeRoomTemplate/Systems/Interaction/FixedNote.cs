using UnityEngine;
using EscapeRoomRevolt.Core;

namespace EscapeRoomRevolt.Systems.Interaction
{
    /// <summary>
    /// A text note that exists physically in the world (e.g. a poster on a wall)
    /// and cannot be picked up. Clicking it displays its content on screen.
    /// </summary>
    public class FixedNote : InteractableBase
    {
        [Header("Note Content")]
        [TextArea(4, 12)]
        [SerializeField] private string _content = "Write your note content here...";
        [SerializeField] private string _prompt = "Read";

        [Header("Behaviour")]
        [SerializeField] private bool _disappearAfterRead = false;

        private bool _hasBeenRead = false;

        public override string InteractionPrompt => _prompt;

        protected override void OnInteract()
        {
            _hasBeenRead = true;

            if (EscapeRoomRevolt.UI.PC.UIManager.Instance != null)
            {
                EscapeRoomRevolt.UI.PC.UIManager.Instance.ShowNoteReader(_content);
            }

            Debug.Log($"[Note] Reading: {name}");

            if (_disappearAfterRead)
                gameObject.SetActive(false);
        }

        public bool HasBeenRead => _hasBeenRead;
        public string Content => _content;
    }
}
