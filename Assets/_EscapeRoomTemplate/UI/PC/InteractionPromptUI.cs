using UnityEngine;
using TMPro;
using EscapeRoomRevolt.Systems.Interaction;

namespace EscapeRoomRevolt.UI.PC
{
    /// <summary>
    /// Displays interaction text (e.g. "Press E to open") when the player
    /// is looking at an IInteractable object.
    /// Needs an InteractionManager in the scene to subscribe to its events.
    /// </summary>
    public class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _promptText;
        [SerializeField] private GameObject _promptContainer;
        [SerializeField] private InteractionManager _interactionManager;

        private void Start()
        {
            if (_interactionManager == null)
            {
                _interactionManager = FindAnyObjectByType<InteractionManager>();
            }

            if (_interactionManager != null)
            {
                _interactionManager.OnFocusChanged += HandleFocusChanged;
            }
            
            HidePrompt();
        }

        private void OnDestroy()
        {
            if (_interactionManager != null)
            {
                _interactionManager.OnFocusChanged -= HandleFocusChanged;
            }
        }

        private void HandleFocusChanged(IInteractable target)
        {
            if (target != null && target.CanInteract)
            {
                ShowPrompt($"[E] {target.InteractionPrompt}");
            }
            else
            {
                HidePrompt();
            }
        }

        private void ShowPrompt(string message)
        {
            if (_promptText != null) _promptText.text = message;
            if (_promptContainer != null) _promptContainer.SetActive(true);
        }

        private void HidePrompt()
        {
            if (_promptContainer != null) _promptContainer.SetActive(false);
        }
    }
}
