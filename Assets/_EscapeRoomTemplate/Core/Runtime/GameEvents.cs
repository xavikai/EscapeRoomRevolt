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

    /// <summary>Fired when the camcorder completes a recordable evidence subject.</summary>
    public struct OnEvidenceRecorded
    {
        public string evidenceId;
        public string title;
    }

    /// <summary>Fired whenever the global game flow changes state.</summary>
    public struct OnGameFlowStateChanged
    {
        public EscapeRoomRevolt.Core.Flow.GameFlowState state;
    }

    /// <summary>Fired once when a victory or defeat condition ends the game.</summary>
    public struct OnGameEnded
    {
        public EscapeRoomRevolt.Core.Flow.GameOutcome outcome;
        public string endingId;
        public string title;
        public string message;
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

    // ─────────────────────────────────────────────
    //  UI STATE EVENTS — published by the UI layer so Systems/Player never
    //  need to reach into UIManager/UIToolkitMenuController singletons to
    //  know whether gameplay input should be blocked. Prefer reading the
    //  cached EscapeRoomRevolt.Core.GameplayBlockState instead of
    //  subscribing to these directly.
    // ─────────────────────────────────────────────

    /// <summary>Fired by GameplayUIController when a gameplay modal (inventory, note, keypad, examiner) opens or closes.</summary>
    public struct OnGameplayUIBlockingChanged
    {
        public bool isBlocking;
    }

    /// <summary>Fired by UIToolkitMenuController when the active menu screen changes to/from Hidden.</summary>
    public struct OnMenuUIBlockingChanged
    {
        public bool isBlocking;
    }

    // ─────────────────────────────────────────────
    //  UI REQUEST EVENTS — published by Systems/Player to ask the UI layer
    //  to do something, instead of calling UIManager/UIToolkitMenuController
    //  directly. GameplayUIController/UIToolkitMenuController subscribe and
    //  perform the action; publishing with no listener is a harmless no-op.
    // ─────────────────────────────────────────────

    /// <summary>Requests the gameplay UI to show a subtitle line.</summary>
    public struct RequestShowSubtitle
    {
        public string text;
    }

    /// <summary>Requests the gameplay UI to hide the current subtitle.</summary>
    public struct RequestHideSubtitle { }

    /// <summary>Requests the gameplay UI to open the note reader with the given content.</summary>
    public struct RequestShowNoteReader
    {
        public string content;
    }

    /// <summary>Requests the gameplay UI to open the keypad modal for a specific puzzle component.</summary>
    public struct RequestShowKeypad
    {
        public Component puzzle;
    }

    /// <summary>Requests the gameplay UI to toggle the inventory panel.</summary>
    public struct RequestToggleInventory { }

    /// <summary>Requests the gameplay UI to close its topmost modal panel.</summary>
    public struct RequestCloseTopPanel { }

    /// <summary>Requests the menu to toggle the pause screen.</summary>
    public struct RequestTogglePause { }
}
