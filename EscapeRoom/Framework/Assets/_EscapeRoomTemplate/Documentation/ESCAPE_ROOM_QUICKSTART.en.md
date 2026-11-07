# Escape Room: creation and delivery guide

**Language:** [castellano](ESCAPE_ROOM_QUICKSTART.md) · [català](ESCAPE_ROOM_QUICKSTART.ca.md) · English · Step-by-step recipes: [RECIPES.md](RECIPES.md)

This guide is for anyone using the template to create their own game. The scope of this edition is **Escape Room for PC and VR with controllers**. Survival Horror is still in development and is not part of this edition's commercial validation. The results of the checks are in [the audit](../AUDITORIA_ESCAPE_ROOM_2026-09-09.md); the extended reference is in [DOCUMENTACIO_COMPLETA.md](../DOCUMENTACIO_COMPLETA.md).

## 1. Opening the project and playing the sample

1. Install Unity **6000.4.9f1**. For Quest, also install Android Build Support and its SDK, NDK and OpenJDK tools from Unity Hub.
2. Open the folder that contains `Assets`, `Packages` and `ProjectSettings`. Wait for importing and compiling to finish.
3. Keep the versions in `Packages/manifest.json` and `packages-lock.json`. The Player assembly references XRI and XR Core Utils even when building for PC; do not remove the XR packages from this project.
4. Open `Assets/_EscapeRoomTemplate/Scenes/ShowcaseMuseum.unity` for PC, or `ShowcaseMuseumVR.unity` for the VR museum. `VRTemplate` is a minimal scene, not the full demo.
5. Run `Escape Room Framework > Validation > Run Framework Smoke Tests` and `Validate Current Scene`. Read the result in the Console, not just the confirmation that the menu command ran.
6. Press Play. On PC, use WASD and the mouse, E to interact, I for the inventory, H for hints and Esc to close the panel or pause. F5/F9 save/load the quick slot. The three manual slots are independent.

The PC museum contains 13 puzzle controllers (the thirteenth is room 14, `LinkedLightsPuzzle`); `ShowcaseMuseumVR` contains the previous 12 and does not include room 14 yet. For a guided, interactive tour of each mechanic, open [`docs/guia/index.html`](../../../docs/guia/index.html) in the browser. The number wheels use `StatePuzzle`; the melody uses `SequencePuzzle`. They are not separate solvers. `LockedOffice` and `LockedOfficeVR` contain an alternative sample with two code panels.

## 2. Creating your own room

1. Create a folder of your own, for example `Assets/MyEscapeRoom`, and keep your scene, data and art there. Keep the sample scenes as a reference.
2. Select `Configuration > Use Escape Room Profile`. The museums include a local flashlight exception for their combination demo; a new scene does not inherit that exception.
3. Use `Setup > Instantiate Game Manager` and the PC player, or instantiate `Prefabs/Player_VR.prefab`. There must be only one active player for the chosen platform. Add a floor with a collider, lighting and physical boundaries.
4. Create objects with the `Create` menu. Keep components, colliders and events on the logical root. Change the art under `ModelSocket` or the visual child indicated by the prefab.
5. Create your own `PuzzleDefinition` and `HintData` for each puzzle. Assign the definition to the controller and the hints to the definition. Write gradual hints: context, pointer and solution.
6. Check IDs: every persistent entity must have a unique `SaveId`. A `PuzzleDefinition.PersistentId` identifies the puzzle when saving. Duplicating an object or a definition can copy its ID: review and change that ID before using both copies together. Reuse one `InventoryItemData` for several units of the same item; create a different `ItemId` for a different type of object.
7. Add the scene to the list of enabled scenes in Build Profiles. Configure `Resources/GameFlowSettings.asset`: `First Gameplay Scene` for New Game and `Main Menu Scene` for returning to the menu. Use unique names or full paths.
8. Validate the scene and test the whole playthrough from start to finish, including saving midway through each mechanism, quitting and loading.

### Example: a code that opens a door

1. Create a code panel and a door from the `Create` menu.
2. In `CodePanelPuzzle`, set `Correct Code = 3142`, `Max Code Length = 4` and enable `Auto Check When Full`.
3. Each button must call `InputDigit` with a single character. The clear button uses `C` or `ClearInput`; if you disable automatic checking, add a button that calls `SubmitCode`.
4. In the controller's `On Solved`, add the door and select its public open method from the drop-down. The door must have its own persistent ID.
5. Add a visible clue that lets the player deduce 3142. Do not enable the random variant if the written clue still shows a fixed solution.
6. Test a wrong code, the correct one, saving with the door open and loading again. `On Solved` represents a new resolution; it is not emitted again on load. The door must restore its state through its own save data.

## 3. Choosing and configuring mechanics

Names in backticks are fields or methods of the component. You can connect public methods through Inspector UnityEvents when their signature is compatible.

| Mechanic | Configuration and input | Minimum check |
|---|---|---|
| Code: `CodePanelPuzzle` | Code, length, automatic check. Buttons → `InputDigit`; confirm → `SubmitCode`. | An error clears the attempt; success fires the event once. |
| Sequence: `SequencePuzzle` | `Correct Sequence` with ordered IDs. Each button/lever → `InputStep(id)` or a `SequenceStepButton`. | An error restarts the attempt. Saving after the first step and loading lets you continue from the second. |
| States and wheels: `StatePuzzle` | `Conditions` list: a `SteppedPositioner` and its `Required Index`, starting at zero. | All positions must match. A missing reference prevents solving. The puzzle saves the position of each lever/wheel: on load, a half-entered combination is kept. |
| Number wheels | `Create > Number Wheels Puzzle`; between 2 and 8 wheels. Edit with `NumberWheelsPuzzleAuthoring` and rebuild from its Inspector. | PC: enter focus and press ▲/▼. VR: equivalent physical buttons. Check the code after resizing. |
| Inventory item: `SocketPuzzle` | `Required Item Id`, optional consumption and a visual prefab with a placement point. Your inventory control calls `TryInsertItem` after checking that the player owns the item. | Rejects a wrong ID; when loaded in the solved state, only one visual piece appears. |
| Receiver with UI: `ItemReceiver` | Assign `Required Item`, selection policy and `On Item Accepted`. This is the authoring option with built-in inventory selection. | Does not open without the item; using it consumes only when appropriate. Do not connect two consumptions for the same action. |
| Physical objects: `PhysicsGrabbable` + `PhysicsSocket` | Matching Rigidbody, collider, `PickableItem.Data` and `Required Item Id`; the socket has a trigger and a snap point. | Only snaps when released. A locked object cannot be grabbed again. This socket does not implement `ISaveable`: for a persistent result, connect your own saveable state or use the placement puzzle. |
| Placement: `PlacementPuzzle` | Rules `pieceId → correctSocketId`; pieces with `GrabbablePiece` and `PieceSocketReceiver` receivers linked to the same puzzle. | Test swapped pieces, removal before solving and loading after solving. Piece and socket IDs must match exactly. Loose pieces are not saved: when loading an unsolved board they return to their starting position. |
| Throwing: `ThrowPuzzle` | Configure the required `ThrowTarget` targets and throwable physical objects. | All required targets must be hit. Make sure the pieces can be recovered. |
| Sliding: `SlidingPuzzle` | Rows, columns, empty cell, shuffle moves; presentation with `SlidingBoardView` and `SlidingTileButton`. | Only a tile next to the gap moves; the loaded state matches the saved one. |
| Pipes: `PipePuzzle` | Tiles with ID, row, column, connections and initial rotation; configure source and destination. `PipeTileButton` → `RotateTile`. | Check real connectivity from source to destination, not just the appearance. The solver needs an interactive presentation. |
| Light circuit: `LinkedLightsPuzzle` | `Nodes` list: initial state, target state, indices toggled by each button, and indicator. Each button → `Press(index)` (`LinkedLightButton`). | Saves partial progress; `ResetPuzzle` returns to the initial state. Check that the target is reachable. |
| Group: `MultiStagePuzzle` | List of child puzzles and interaction roots; free or mandatory order and locking of later ones. | The children remain visible; the group solves once when all are complete. The final reward is connected to the group. |
| Melody: `MelodyPlayer` | Audio presentation of the steps of a sequence. | Add an equivalent visual clue; avoid relying on hearing alone. |
| Notes and examining | `InteractableNote` for reading; `InventoryItemData` with read/examine data and `ExamineHotspot` for areas of the model. | Readable text, closing returns control, and a hotspot does not fire through another panel. |
| Switches, doors and drawers | `InteractableTrigger`, `InteractableToggle`, `SteppedPositioner`, `Door`; configure events and travel. | No duplicated events, no passing through boundaries, same usage on PC and VR. `InteractableToggle` and `InteractableTrigger` save their state; enable `Invoke Event On Load` on the switch if a light without its own save data must recover its state. To rotate on a hinge, use an empty object placed at the hinge as the `Visual Transform`. |
| Hints | `HintData` in the definition; optional `HintZoneTrigger` zones. | Hints relevant to the active puzzle and withdrawn once solved. |
| Time and moving hazard | `GameOverTimer.StartTimer` and `MovingHazard.StartHazard`, activated by a button or `EventTriggerZone`. 3D markers define the hazard's path. | Pause freezes progress, the HUD reflects the time, failure shows the results. They are independent mechanisms. |
| Objectives and ending | `ObjectiveSet`/`ObjectiveManager`, prerequisites without cycles; `GameEndTrigger` and `EndingDefinition`. | The game does not end before the last objective; Retry lets the player pick up the items again. |
| Changing rooms | `RoomPortal`, enabled destination scene, `RoomSpawnPoint` with ID. | In Single mode there is no full cache of previous rooms. In Additive mode, avoid duplicated managers and players. |

### Resetting a puzzle is not the same as resetting the world

`ResetPuzzle` returns the controller to unsolved and clears its specific state. It does not automatically revert a door connected by an event, does not return consumed items and does not rebuild any object destroyed by your own logic. If you offer a reset button, also connect the restoration of its surroundings. To start from scratch, use New Game or the scene's Retry.

## 4. Inventory, combining and saving

1. Create an `InventoryItemData` with ID, name, icon and the actions it should allow. Add it to the `ItemCatalog` used in the game; the references must make it into the build.
2. To pick it up, assign its data to a `PickableItem`. To combine, configure `Combinations` with the other item and the result. Also include the result in the catalog.
3. Test the combination with a full inventory and with multiple quantities. Do not use an essential item that can be lost without a way to recover it.
4. Save before and after consuming a key, solving, opening a door or completing an objective. Close the executable and load, in addition to testing loading within the same session.
5. The files are in `Application.persistentDataPath/SaveSlots`. Each slot has a JSON file, a thumbnail and, after an overwrite, a `.bak` copy. Do not delete IDs of already published content if you want to keep compatibility with saved games.
6. A backup can recover a main JSON that is missing or structurally invalid. This does not repair arbitrary incorrect states inside each component. Keep external copies for migration testing.
7. The PC and VR slots of these samples must not be advertised as interchangeable: their scene paths and some player components are different.

## 5. Preparing VR with controllers

1. Start from `ShowcaseMuseumVR` to study the mechanics, or from `VRTemplate` for a minimal scene.
2. Check OpenXR with `Setup > Configure OpenXR (PC + Android)` and the Project Validation window. Keep the profiles for the controllers you are going to support.
3. For your own scene, prepare the interactables with `Setup > Prepare Current Scene Interactables for VR`. Then review each object's references, and do not regenerate a customized demo without saving a copy.
4. The rig uses `VRPlayerPlatformAdapter`, the 3D UI Toolkit presentation and its pointer bridges. Keep the head and both hand references. The VR player must be the only active player.
5. The `VRHardwareInteractor` path uses the trigger to activate and grip/trigger to hold physical objects. Releasing both lets go of the object; throwing depends on the hand's movement and on `Can Be Thrown`. Sockets wait until the object is released.
6. The XRI path uses `VRInteractionBridge`. Do not configure two different actions so that a single press activates a mechanism twice. Pay special attention to switches, ▲/▼ buttons and picking up.
7. To test without a headset, enable the simulator object indicated in `VRTemplate`. The simulation does not verify the physical buttons and does not replace testing the executable on the device. Disable it for the headset delivery.
8. Review `Resources/VRComfortSettings.asset`: locomotion, snap/continuous turning and preferences. Test seated and standing heights, reachability of all controls and panel scale.
9. Do not use feedback cameras or forced head movements as a requirement for solving. The puzzle base skips its camera cuts in VR; any camera or cutscene of your own needs the same treatment.

The Quest package from `ReleaseBuilder` starts directly in `ShowcaseMuseumVR` and does not include a main menu scene. The pause/results menu omits the return to menu when that scene is not available. For a VR game with a main menu, create a VR-compatible menu scene, include it in the build and configure its paths: do not simply reuse the PC menu.

`Build > Release > Build Windows` generates the desktop demo without starting XR and restores the editor's XR configuration when it finishes. A PCVR executable requires its own list of VR scenes and must keep XR initialization enabled; do not use this desktop command for a PCVR delivery. On Android, the input read priority indicated by the Meta Quest/OpenXR validation has been applied.

## 6. Before delivering to a client

| Check | Acceptance criterion |
|---|---|
| Clean install | Import the package into another folder with the documented version; it does not depend on Library or on private development tools. |
| PC playthrough | Complete the museum and your own room without the debug console; test failure and success in each mechanic. |
| Saving | Save, close the executable and load midway through each mechanism, after picking up/consuming and after changing scenes. |
| VR on device | Record headset, controllers, runtime, build version and the result of each row of the VR matrix. |
| Performance | Measure CPU/GPU, memory and frame stability on the target hardware with the final scene. This audit does not certify a budget. |
| Content | Complete the provenance and redistribution terms in `ThirdPartyNotices.md`; replace material whose provenance cannot be verified. |
| Languages | Also review notes, prompts, hints, puzzles and endings; the language selector does not guarantee that all your own content is translated. |
| Package | Keep `.meta` files, scenes, data, prefabs, shaders, UI and required dependencies. Exclude builds, caches, logs and MCP tools from the delivery to the buyer. |

Do not remove the Survival folder on its own when packaging: there are currently compile-time references from shared systems. The Escape Room profile disables its optional features; physically separating those assemblies requires additional work.

### VR matrix to be filled in per device

| Case | Result / device / build |
|---|---|
| Startup, tracking, audio and recentering | Pending |
| Both hands: interaction, grab, release and throw | Pending |
| All rooms; targets, placement and wheels | Pending |
| Inventory, combining, notes, examining and hotspots | Pending |
| Pause, settings, save/load and closing a panel without activating the world | Pending |
| Teleport, turning and chosen locomotion; seated/standing | Pending |
| Ending, defeat, Retry and restored pickable objects | Pending |
| Tracking loss, disconnected controller and suspend/resume | Pending |
| Sustained performance during a full playthrough | Pending |

## 7. Troubleshooting common problems

| Symptom | What to check |
|---|---|
| It does not open from Hub | Check that no other Unity instance or test has the same project open. Do not delete the lock while it is still active. |
| "Missing script" after importing | Editor version, resolved packages and compilation errors; keep `.meta` files and assembly references. |
| The puzzle does not solve | IDs, order, zero-based indices, definition and complete conditions/references. |
| A door closes again on load | Its own save data/ID and visual restoration; do not rely on `On Solved` being emitted again. |
| VR does not show the museum | Open `ShowcaseMuseumVR`; `VRTemplate` is only the minimal startup scene. |
| A VR control activates twice | Check for duplicated events and the rig's XRI/hardware paths. |
| A required object is missing | Piece recovery, physical boundaries and throwing options; check for duplicated IDs and consumption rules. |
| The menu cannot load a scene | Include the destination in the list used by that build; check the flow configuration. |

For advanced fields and code extensions, see [the programming guide](../PROGRAMMING_GUIDE.md) and [the complete reference](../DOCUMENTACIO_COMPLETA.md).
