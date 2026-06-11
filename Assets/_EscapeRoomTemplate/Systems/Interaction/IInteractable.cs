namespace EscapeRoomRevolt.Systems.Interaction
{
    public enum CursorType
    {
        Default,
        Hand,
        Eye,
        Puzzle,
        Exit
    }

    /// <summary>
    /// Contract that every interactable object in the game must implement.
    /// Attach this to doors, items, notes, puzzles, drawers, etc.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Display name shown in the interaction UI prompt.</summary>
        string InteractionPrompt { get; }

        /// <summary>The type of crosshair to show when looking at this object.</summary>
        CursorType InteractionCursor { get; }

        /// <summary>Whether the object can currently be interacted with.</summary>
        bool CanInteract { get; }

        /// <summary>Called when the player triggers the interaction.</summary>
        void Interact();

        /// <summary>Called when the player's crosshair enters this object's range.</summary>
        void OnFocusEnter();

        /// <summary>Called when the player's crosshair leaves this object's range.</summary>
        void OnFocusExit();
    }
}
