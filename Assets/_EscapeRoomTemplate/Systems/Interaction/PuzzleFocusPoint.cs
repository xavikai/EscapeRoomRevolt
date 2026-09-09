using UnityEngine;
using EscapeRoomRevolt.Core;
using EscapeRoomRevolt.Player;
using EscapeRoomRevolt.Systems.Puzzle;

namespace EscapeRoomRevolt.Systems.Interaction
{
    /// <summary>
    /// Frames a puzzle that is solved in the world: the view cuts to a camera pointed at it and the
    /// cursor is freed, so a board of small pieces is comfortable to work with on a mouse without
    /// ever leaving the scene.
    ///
    /// Deliberately not a UI modal. Time keeps running, enemies keep moving and sound keeps playing,
    /// which is the whole reason a puzzle in a horror game is worth solving in the world rather than
    /// on a panel: the player has to weigh finishing it against what is walking towards them. It
    /// also means one implementation serves PC and VR — in a headset the player simply walks up to
    /// the board, so this component stands aside entirely.
    ///
    /// Drive it from an InteractableTrigger on the board (Enter or Toggle); exit is right-click,
    /// solving the puzzle, or anything that opens a UI panel over the top.
    /// </summary>
    public sealed class PuzzleFocusPoint : MonoBehaviour
    {
        [Header("Focus")]
        [Tooltip("Camera pointed at the puzzle. Keep it inside the InteractionManager's interaction range of the pieces (2.5 m by default), or the player will not be able to click them from here.")]
        [SerializeField] private Camera _focusCamera;
        [Tooltip("Rendered over the player's camera. Raise only if something else already draws on top.")]
        [SerializeField] private float _cameraDepth = 1f;

        [Header("Exit")]
        [Tooltip("Optional. When this puzzle is solved the view returns to the player on its own, so nobody is left staring at a finished board.")]
        [SerializeField] private PuzzleController _puzzle;
        [Tooltip("How long to stay on the puzzle after it is solved, so the player sees it complete.")]
        [SerializeField, Min(0f)] private float _exitDelayAfterSolved = 1.4f;

        private bool _isFocused;
        private bool _secondaryWasHeld;
        private float _solvedTimer;

        public bool IsFocused => _isFocused;

        /// <summary>True in a headset, where taking the camera off the player's head is both unnecessary and a reliable way to make them ill.</summary>
        private static bool IsVirtualReality =>
            PlayerPlatformRegistry.Current != null &&
            PlayerPlatformRegistry.Current.Platform == PlayerPlatform.VirtualReality;

        public void Toggle()
        {
            if (_isFocused) Exit();
            else Enter();
        }

        public void Enter()
        {
            if (_isFocused) return;
            if (IsVirtualReality)
            {
                if (_puzzle != null)
                    EventBus.Publish(new RequestShowNumberWheels { puzzle = _puzzle });
                else
                    Debug.LogWarning($"[PuzzleFocusPoint] {name} cannot open a VR puzzle panel without an assigned puzzle.", this);
                return;
            }
            if (_focusCamera == null)
            {
                Debug.LogWarning($"[PuzzleFocusPoint] {name} has no focus camera assigned.", this);
                return;
            }

            _isFocused = true;
            _solvedTimer = 0f;
            // Consumed already if focus was entered with the same button that exits it.
            _secondaryWasHeld = true;

            _focusCamera.gameObject.SetActive(true);
            _focusCamera.depth = _cameraDepth;
            InteractionManager.Instance?.SetOverrideCamera(_focusCamera);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // The player stays where they are and the world keeps running; only their control of the
            // camera is borrowed.
            PlayerPlatformRegistry.Current?.SetLookBlocked(true);
            PlayerPlatformRegistry.Current?.SetMovementBlocked(true);
        }

        public void Exit()
        {
            if (!_isFocused) return;
            _isFocused = false;

            if (_focusCamera != null) _focusCamera.gameObject.SetActive(false);
            InteractionManager.Instance?.ClearOverrideCamera();

            PlayerPlatformRegistry.Current?.SetLookBlocked(false);
            PlayerPlatformRegistry.Current?.SetMovementBlocked(false);

            // A panel opened over the focus owns the cursor; grabbing it back would trap the player
            // in a menu they cannot click.
            if (GameplayBlockState.IsBlocking) return;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (!_isFocused) return;

            // A pause menu or inventory opening over the top takes precedence.
            if (GameplayBlockState.IsBlocking) { Exit(); return; }

            var input = EscapeRoomRevolt.Core.Input.InputRouter.Instance;
            bool secondaryHeld = input != null && input.SecondaryActionHeld;
            if (secondaryHeld && !_secondaryWasHeld) { _secondaryWasHeld = true; Exit(); return; }
            _secondaryWasHeld = secondaryHeld;

            if (_puzzle == null || !_puzzle.IsSolved) return;
            _solvedTimer += Time.deltaTime;
            if (_solvedTimer >= _exitDelayAfterSolved) Exit();
        }

        /// <summary>Leaving the player frozen with a stray camera enabled is the one failure here that a player cannot recover from.</summary>
        private void OnDisable() => Exit();
    }
}
