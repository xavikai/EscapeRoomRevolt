using UnityEngine;
using EscapeRoomRevolt.Core;

namespace EscapeRoomRevolt.Systems.Interaction
{
    /// <summary>
    /// A readable note, document or diary entry.
    /// When interacted with, fires OnNoteRead so the UI can display the content.
    ///
    /// Publishes: OnNoteRead
    /// </summary>
    public class ReadableNote : InteractableBase
    {
        [Header("Note Content")]
        [SerializeField] private string _noteId = "";
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

            EventBus.Publish(new OnNoteRead
            {
                noteId = string.IsNullOrEmpty(_noteId) ? SaveId : _noteId,
                content = _content
            });

            Debug.Log($"[Note] Reading: {name}");

            if (_disappearAfterRead)
                gameObject.SetActive(false);
        }

        public bool HasBeenRead => _hasBeenRead;
        public string Content => _content;
    }
}
