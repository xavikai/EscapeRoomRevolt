using UnityEngine;

namespace EscapeRoomRevolt.Core
{
    // ─────────────────────────────────────────────
    //  GAME EVENTS — Define all game events here
    //  All events must be structs (value types)
    // ─────────────────────────────────────────────

    /// <summary>Fired when the player picks up an item.</summary>
    public struct OnItemPickedUp
    {
        public string itemId;
        public string itemName;
    }

    /// <summary>Fired when the player uses/removes an item from inventory.</summary>
    public struct OnItemUsed
    {
        public string itemId;
    }

    /// <summary>Fired when a puzzle is solved.</summary>
    public struct OnPuzzleSolved
    {
        public string puzzleId;
    }

    /// <summary>Fired when a puzzle fails (wrong code, wrong sequence, etc.).</summary>
    public struct OnPuzzleFailed
    {
        public string puzzleId;
        public string reason;
    }

    /// <summary>Fired when a door or container changes its locked state.</summary>
    public struct OnLockStateChanged
    {
        public string lockableId;
        public bool isLocked;
    }

    /// <summary>Fired when the player reads a note or document.</summary>
    public struct OnNoteRead
    {
        public string noteId;
        public string content;
    }

    /// <summary>Fired when an objective is completed.</summary>
    public struct OnObjectiveCompleted
    {
        public string objectiveId;
    }

    /// <summary>Fired when all objectives are complete and the room is escaped.</summary>
    public struct OnRoomEscaped
    {
        public string roomId;
        public float completionTimeSeconds;
    }

    /// <summary>Fired when the game is saved.</summary>
    public struct OnGameSaved
    {
        public string slotId;
    }

    /// <summary>Fired when the game is loaded.</summary>
    public struct OnGameLoaded
    {
        public string slotId;
    }

    /// <summary>Fired when the player interacts with any interactable object.</summary>
    public struct OnInteractionPerformed
    {
        public string interactableId;
        public GameObject target;
    }
}
