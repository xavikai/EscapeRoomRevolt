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

        private IInteractable _currentTarget;

        private void Start()
        {
            if (_promptText != null)
            {
                _promptText.fontSize = 24; // Make the text much smaller
            }

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
            _currentTarget = target;
        }

        private void Update()
        {
            bool isFocusMode = Cursor.lockState == CursorLockMode.None || Cursor.visible;

            if (isFocusMode || _currentTarget == null || !_currentTarget.CanInteract)
            {
                if (_promptContainer != null && _promptContainer.activeSelf)
                {
                    HidePrompt();
                }
            }
            else
            {
                if (_promptContainer != null && !_promptContainer.activeSelf)
                {
                    _promptContainer.SetActive(true);
                }
                if (_promptText != null)
                {
                    _promptText.text = $"[E] {_currentTarget.InteractionPrompt}";
                }
            }
        }

        // Helper methods kept for compatibility, though Update now handles visibility
        private void HidePrompt()
        {
            if (_promptContainer != null) _promptContainer.SetActive(false);
        }
    }
}
