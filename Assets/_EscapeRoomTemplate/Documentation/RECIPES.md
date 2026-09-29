# Step-by-step recipes

**Language:** [català](RECEPTES.md) · [castellano](RECETAS.md) · [English](RECIPES.md)

Each recipe starts from an empty scene with the player already placed (`Setup > Instantiate Game Manager` and the PC or VR player). To try them interactively, open the guide [`docs/guia/index.html`](../../../docs/guia/index.html) > *Step-by-step recipes*.

- Puzzles: [Code keypad](#code) · [Sequence and melody](#seq) · [Levers and switches (StatePuzzle)](#state) · [Number wheels (lock)](#wheels) · [Pipes](#pipe) · [Sliding puzzle](#slide) · [Linked lights](#lights) · [Throw at targets](#throw) · [Piece placement](#place) · [Chained puzzles](#multi)
- Room systems: [Items, inventory, combining and examine](#items) · [Doors, drawers and cabinets](#doors) · [Hints and narration](#hints) · [Objectives and game ending](#objectives) · [Timer and moving hazard](#hazard) · [Changing room or scene](#rooms)

## Puzzles

<a id="code"></a>
### Code keypad

Menu: `Escape Room Framework > Create > Puzzles > Keypad Panel` · Museum room 2

**Steps**

1. Run Escape Room Framework > Create > Puzzles > Keypad Panel. It creates NewKeypad with CodePanelPuzzle, InteractableKeypad, a display, an LED and eleven 3D buttons (1–9, C, 0) already wired to the puzzle.
2. Place it on a wall by moving only the NewKeypad root. Don't scale the buttons one by one.
3. In CodePanelPuzzle type the Correct Code (for example 3142), set Max Code Length to the same number of digits and keep Auto Check When Full on.
4. Create the puzzle data: in the Project panel, right click > Create > Escape Room Framework > Puzzles > Puzzle Definition. Give it a unique Persistent Id (for example room1_keypad), a Display Name and the Objective. Then create hints with Create > Escape Room Framework > Hints > Hint Data (three hints: context, nudge and solution) and drag them into the definition's Hints field.
5. Select the puzzle root and drag the definition into the component's Definition field.
6. Create the reward: Escape Room Framework > Create > Interactables > Door and tick Is Locked on the door. Select the puzzle, Ctrl+click the door and run Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle. On Solved will now call Door.Unlock and Door.ForceOpen.
7. Hide the code clue: Escape Room Framework > Create > Interactables > Note and write it in Note Content.

**How to test it**

- A wrong code flashes the LED red and clears the attempt; the right one turns it green and opens the door.
- Test it in Play: save with F5 halfway through and once solved, then load with F9. The state must match and the door must stay open.

**Watch out for**

- Randomize Code rolls a new code every playthrough: only use it if the clue is dynamic too.
- On Solved only runs when the player solves the puzzle, not when a game is loaded: the door restores itself from its own save data.

<a id="seq"></a>
### Sequence and melody

Menu: `Escape Room Framework > Create > Puzzles > Sequence Puzzle` · Museum room 4 · 9

**Steps**

1. Run Escape Room Framework > Create > Puzzles > Sequence Puzzle. It creates three buttons (red, green, blue), each with a SequenceStepButton, and the correct order red → green → blue.
2. SequencePuzzle's Correct Sequence list holds the IDs in order. Each SequenceStepButton's Step Id must match one of those IDs exactly.
3. To add a step, duplicate a button, change its Step Id and add that ID to Correct Sequence. An ID may appear more than once.
4. Melody (optional): give each button an AudioSource with its note, add a MelodyPlayer to the puzzle and fill Notes in the right order. Create a Generic Trigger "Listen to the melody" whose On Interact Event calls MelodyPlayer.Play.
5. Create the puzzle data: in the Project panel, right click > Create > Escape Room Framework > Puzzles > Puzzle Definition. Give it a unique Persistent Id (for example room1_keypad), a Display Name and the Objective. Then create hints with Create > Escape Room Framework > Hints > Hint Data (three hints: context, nudge and solution) and drag them into the definition's Hints field.
6. Select the puzzle root and drag the definition into the component's Definition field.
7. Create the reward: Escape Room Framework > Create > Interactables > Door and tick Is Locked on the door. Select the puzzle, Ctrl+click the door and run Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle. On Solved will now call Door.Unlock and Door.ForceOpen.

**How to test it**

- A wrong step restarts the attempt and fires On Failed.
- Save after the first correct step and load: you must be able to continue from the second.
- Test it in Play: save with F5 halfway through and once solved, then load with F9. The state must match and the door must stay open.

**Watch out for**

- If the only clue is audio, add a visual one: not everyone can tell the notes apart.
- Randomize Order shuffles the order each playthrough; the clue must show the real order.

<a id="state"></a>
### Levers and switches (StatePuzzle)

Menu: `Escape Room Framework > Create > Puzzles > State Puzzle` · Museum room 5

**Steps**

1. Run Escape Room Framework > Create > Puzzles > State Puzzle. It creates three three-position levers (SteppedPositioner + InteractableCycler) that solve when they read 0, 1 and 2.
2. On each lever, the Positions list defines each position: the text the player sees (Prompt) and the rotation or offset. Starting Index is where it begins.
3. In StatePuzzle, the Conditions list pairs each lever (Positioner) with the position it must show (Required Index, starting at 0).
4. For a two-position switch use Escape Room Framework > Create > Interactables > Multi-Position Lever with two positions. InteractableToggle (Lever/Switch) does not feed this puzzle.
5. Create the puzzle data: in the Project panel, right click > Create > Escape Room Framework > Puzzles > Puzzle Definition. Give it a unique Persistent Id (for example room1_keypad), a Display Name and the Objective. Then create hints with Create > Escape Room Framework > Hints > Hint Data (three hints: context, nudge and solution) and drag them into the definition's Hints field.
6. Select the puzzle root and drag the definition into the component's Definition field.
7. Create the reward: Escape Room Framework > Create > Interactables > Door and tick Is Locked on the door. Select the puzzle, Ctrl+click the door and run Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle. On Solved will now call Door.Unlock and Door.ForceOpen.

**How to test it**

- It solves only when every lever is in position; order does not matter.
- Save with levers halfway and load: positions are kept.
- Test it in Play: save with F5 halfway through and once solved, then load with F9. The state must match and the door must stay open.

**Watch out for**

- A condition without a Positioner prevents solving.

<a id="wheels"></a>
### Number wheels (lock)

Menu: `Escape Room Framework > Create > Puzzles > Number Wheels Puzzle` · Museum room 13

**Steps**

1. Run Escape Room Framework > Create > Puzzles > Number Wheels Puzzle. In the window choose how many wheels (2 to 8) and each digit, then press Create puzzle.
2. It creates a StatePuzzle with a PuzzleFocusPoint (close-up camera), the NumberWheelsPuzzleAuthoring component and ▲/▼ buttons for each wheel.
3. To change the combination or wheel count, edit NumberWheelsPuzzleAuthoring and press Rebuild wheels and layout. The definition and On Solved are kept.
4. To change the look (suitcase, safe), swap the models inside the ReplaceableModelSlot of the casing and the wheels.
5. Create the puzzle data: in the Project panel, right click > Create > Escape Room Framework > Puzzles > Puzzle Definition. Give it a unique Persistent Id (for example room1_keypad), a Display Name and the Objective. Then create hints with Create > Escape Room Framework > Hints > Hint Data (three hints: context, nudge and solution) and drag them into the definition's Hints field.
6. Select the puzzle root and drag the definition into the component's Definition field.
7. Create the reward: Escape Room Framework > Create > Interactables > Door and tick Is Locked on the door. Select the puzzle, Ctrl+click the door and run Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle. On Solved will now call Door.Unlock and Door.ForceOpen.

**How to test it**

- On PC: E enters the close-up view, click ▲/▼, right click leaves. In VR: physical ▲/▼ buttons with the trigger.
- Test it in Play: save with F5 halfway through and once solved, then load with F9. The state must match and the door must stay open.

**Watch out for**

- After changing the wheel count, check the combination is still the one you want.

<a id="pipe"></a>
### Pipes

Menu: `Escape Room Framework > Create > Puzzles > Pipe Puzzle` · Museum room 10

**Steps**

1. Run Escape Room Framework > Create > Puzzles > Pipe Puzzle. It creates NewPipePuzzle with two sample segments (pipe_a and pipe_b) rotated 180°. It is logic only: nothing is visible yet.
2. In the Tiles list, each segment has Tile Id, Row, Column, Open Sides (open sides before rotating: North, East, South, West) and Starting Rotation Steps (0–3, 90° steps). Set Source Tile Id and Sink Tile Id.
3. For each segment, create a clickable object (Escape Room Framework > Create > Interactables > Generic Trigger) and add a PipeTileButton with the puzzle and its Tile Id. In the trigger's On Interact Event call PipeTileButton.Rotate.
4. To make the model turn, add a SteppedPositioner with four positions (0°, 90°, 180°, 270°) to the visual segment and assign it to the PipeTileButton's Visual Positioner.
5. Create the puzzle data: in the Project panel, right click > Create > Escape Room Framework > Puzzles > Puzzle Definition. Give it a unique Persistent Id (for example room1_keypad), a Display Name and the Objective. Then create hints with Create > Escape Room Framework > Hints > Hint Data (three hints: context, nudge and solution) and drag them into the definition's Hints field.
6. Select the puzzle root and drag the definition into the component's Definition field.
7. Create the reward: Escape Room Framework > Create > Interactables > Door and tick Is Locked on the door. Select the puzzle, Ctrl+click the door and run Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle. On Solved will now call Door.Unlock and Door.ForceOpen.

**How to test it**

- It solves when a continuous path links source to sink; once solved, segments stop turning.
- Test it in Play: save with F5 halfway through and once solved, then load with F9. The state must match and the door must stay open.

**Watch out for**

- Randomize Rotations shuffles starting rotations and never starts solved.

<a id="slide"></a>
### Sliding puzzle

Menu: `Escape Room Framework > Create > Puzzles > Sliding Puzzle` · Museum room 8

**Steps**

1. Run Escape Room Framework > Create > Puzzles > Sliding Puzzle. It creates a playable 3×3 board with eight clickable tiles.
2. Select the Board child (SlidingBoardView) and assign a picture to Source Image: it is cut into fragments that show the player the right order.
3. To resize, change Columns, Rows and Hole Cell on SlidingPuzzle and press Rebuild board on Board.
4. Shuffle Move Count sets how many legal moves shuffle the board: it is always solvable.
5. Create the puzzle data: in the Project panel, right click > Create > Escape Room Framework > Puzzles > Puzzle Definition. Give it a unique Persistent Id (for example room1_keypad), a Display Name and the Objective. Then create hints with Create > Escape Room Framework > Hints > Hint Data (three hints: context, nudge and solution) and drag them into the definition's Hints field.
6. Select the puzzle root and drag the definition into the component's Definition field.
7. Create the reward: Escape Room Framework > Create > Interactables > Door and tick Is Locked on the door. Select the puzzle, Ctrl+click the door and run Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle. On Solved will now call Door.Unlock and Door.ForceOpen.

**How to test it**

- Only a tile next to the hole moves; others give a small nudge.
- Test it in Play: save with F5 halfway through and once solved, then load with F9. The state must match and the door must stay open.

**Watch out for**


<a id="lights"></a>
### Linked lights

Menu: `Assets/_EscapeRoomTemplate/Prefabs/LinkedLightsPuzzleKit.prefab` · Museum room 14

**Steps**

1. Drag Prefabs/LinkedLightsPuzzleKit.prefab into the scene. It includes the panel, five buttons (LinkedLightButton), labels and a reset button.
2. In LinkedLightsPuzzle > Nodes, each light has Initially On, Target On, Connections (indices its button toggles, from 0; include its own index if it should toggle) and Indicator.
3. The prefab shares the demo_linked_lights definition: duplicate it, change the Persistent Id and assign the copy. Do this for every circuit in the scene.
4. Create the reward: Escape Room Framework > Create > Interactables > Door and tick Is Locked on the door. Select the puzzle, Ctrl+click the door and run Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle. On Solved will now call Door.Unlock and Door.ForceOpen.

**How to test it**

- With the sample setup it solves by pressing 1, 3 and 5. The reset button restores the initial state.
- Test it in Play: save with F5 halfway through and once solved, then load with F9. The state must match and the door must stay open.

**Watch out for**

- Make sure it is solvable: start from the goal and apply a few presses to get the initial state.

<a id="throw"></a>
### Throw at targets

Menu: `Escape Room Framework > Create > Puzzles > Throw Puzzle` · Museum room 6

**Steps**

1. Run Escape Room Framework > Create > Puzzles > Throw Puzzle. It creates three targets with ThrowTarget (target_1, target_2, target_3) listed in Target Ids.
2. Add objects to throw: Escape Room Framework > Create > Interactables > Physics Grabbable, with Can Be Thrown on.
3. On each target, Min Impact Speed (m/s) stops gentle bumps from counting.
4. Enclose the area with colliders so objects can't get lost.
5. Create the puzzle data: in the Project panel, right click > Create > Escape Room Framework > Puzzles > Puzzle Definition. Give it a unique Persistent Id (for example room1_keypad), a Display Name and the Objective. Then create hints with Create > Escape Room Framework > Hints > Hint Data (three hints: context, nudge and solution) and drag them into the definition's Hints field.
6. Select the puzzle root and drag the definition into the component's Definition field.
7. Create the reward: Escape Room Framework > Create > Interactables > Door and tick Is Locked on the door. Select the puzzle, Ctrl+click the door and run Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle. On Solved will now call Door.Unlock and Door.ForceOpen.

**How to test it**

- On PC: E to grab, left click to throw, Q to drop. In VR: grab with grip and release with a throwing motion.
- Test it in Play: save with F5 halfway through and once solved, then load with F9. The state must match and the door must stay open.

**Watch out for**


<a id="place"></a>
### Piece placement

Menu: `Escape Room Framework > Create > Puzzles > Placement Puzzle` · Museum room 7

**Steps**

1. Run Escape Room Framework > Create > Puzzles > Placement Puzzle. It creates two pieces (piece_a, piece_b) and three holders (socket_a, socket_b and the decoy socket_c).
2. In PlacementPuzzle, each rule (Rules) says which holder each piece belongs to: pieceId → correctSocketId.
3. On each piece, GrabbablePiece > Piece Id must match the rule. Return When Lost brings it back if it ends up out of reach.
4. Each holder's PieceSocketReceiver has the puzzle, the Socket Id and a Snap Point. On Piece Seated / Removed / Locked events are for sound and animation.
5. Create the puzzle data: in the Project panel, right click > Create > Escape Room Framework > Puzzles > Puzzle Definition. Give it a unique Persistent Id (for example room1_keypad), a Display Name and the Objective. Then create hints with Create > Escape Room Framework > Hints > Hint Data (three hints: context, nudge and solution) and drag them into the definition's Hints field.
6. Select the puzzle root and drag the definition into the component's Definition field.
7. Create the reward: Escape Room Framework > Create > Interactables > Door and tick Is Locked on the door. Select the puzzle, Ctrl+click the door and run Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle. On Solved will now call Door.Unlock and Door.ForceOpen.

**How to test it**

- Try swapped pieces, removing them before solving and loading once solved: pieces stay locked in.
- If you save halfway, loose pieces return to their start on load.

**Watch out for**

- Randomize Mapping changes the correct holder each playthrough: the clue must be dynamic.

<a id="multi"></a>
### Chained puzzles

Menu: `Escape Room Framework > Create > Puzzles > Multi-Stage Puzzle` · Museum room 11

**Steps**

1. Run Escape Room Framework > Create > Puzzles > Multi-Stage Puzzle. It creates a group with two visible children: Puzzle01_Sequence and Puzzle02_Levers.
2. In MultiStagePuzzle > Puzzles, each entry has an id, the child puzzle and its interaction root. You can add any other puzzle in the scene.
3. Require Order forces the list order; Lock Future Puzzles keeps later puzzles visible but unresponsive until their turn.
4. Each child needs its own PuzzleDefinition with a unique ID. The group has one too.
5. Wire the door only to the group's On Solved, not the children's (use Link Selected Objects To Puzzle with the group selected).

**How to test it**

- The door opens only when every child is solved.
- Test it in Play: save with F5 halfway through and once solved, then load with F9. The state must match and the door must stay open.

**Watch out for**


## Room systems

<a id="items"></a>
### Items, inventory, combining and examine

Menu: `Create > Escape Room Framework > Inventory > Item` · Museum room 3

**Steps**

1. Create the item: in Project, Create > Escape Room Framework > Inventory > Item. Fill Item Id (unique, lowercase, for example office_key), Display Name, Description and Icon.
2. Assign World Prefab (the 3D model) and enable Can Examine if it can be examined. For a note, enable Is Readable and write Note Content.
3. Add it to the ItemCatalog used by InventoryManager (on the GameManager; ScriptableObjects/DefaultItemCatalog by default).
4. Place it in the world: Escape Room Framework > Create > Interactables > Pickable Item and assign it to Item Data.
5. Combining: on item A, add a Combinations entry with Combine With = B and Result Item = C (add C to the catalog too). Destroy This / Destroy Other decide what disappears.
6. Examine hotspots: open the World Prefab, select it and run Escape Room Framework > Create > Inventory > Examine Hotspot. Set Hotspot Id, Unrevealed Prompt, Revealed Description and, if needed, Revealed Item.
7. To use the item on a lock: on the door, Is Locked + Required Item Id = the Item Id. Or Escape Room Framework > Create > Interactables > Item Receiver with Required Item and On Item Accepted.

**How to test it**

- I opens the inventory. Test combining with a full inventory and with stacks.
- Save before and after using a key: a picked-up item does not respawn.

**Watch out for**

- Never reuse an Item Id for different items. For several units of the same item, reuse the same asset.

<a id="doors"></a>
### Doors, drawers and cabinets

Menu: `Escape Room Framework > Create > Interactables > Door · Drawer · Cabinet` · Museum room 1

**Steps**

1. Run Escape Room Framework > Create > Interactables > Door (or Drawer or Cabinet). They all use the Door component.
2. Movement Type: Pivot for doors and cabinets (turns around Custom Pivot by Open Angle), Slide for drawers (Slide Offset).
3. To lock it: Is Locked. With a key: Required Item Id and Consume Required Item. Item Use Policy decides how the key is offered (Offer Compatible, Selected Only or Auto Use Single).
4. From any event you can call Unlock, Lock, ForceOpen or ForceClose.
5. Swap the model inside the visual child; don't touch the collider or the root components.

**How to test it**

- The door saves whether it is locked and how far open it is.

**Watch out for**

- If you duplicate a door, change its Save Id.

<a id="hints"></a>
### Hints and narration

Menu: `Create > Escape Room Framework > Hints > Hint Data`

**Steps**

1. Create a Hint Data: Delay Before First Hint (seconds to the first hint), Delay Between Hints and the Hints list, from vague to solution. Each hint can have audio.
2. Assign it to the PuzzleDefinition's Hints field: it starts when the player begins the puzzle and stops when it is solved.
3. For area hints: Escape Room Framework > Create > Triggers > Hint Zone, assign Puzzle Hint Data and optionally Clear On Exit.
4. For narration on entering a place: Escape Room Framework > Create > Triggers > Narrative Trigger with Play Mode Once, Always or ProgressiveHints.

**How to test it**

- The player can ask for the next hint with H. Hints appear as subtitles and follow the Subtitles setting.

**Watch out for**

- Write hints in Spanish (the catalog key) and translate them in DefaultLocalizationCatalog if the game is multilingual.

<a id="objectives"></a>
### Objectives and game ending

Menu: `Escape Room Framework > Create > Flow > Objective Manager · Game End Trigger`

**Steps**

1. Create an Ending Definition (Create > Escape Room Framework > Ending Definition): Ending Id, Outcome (Victory or Defeat), Title and Message.
2. Create one Objective Definition per objective: Objective Id, Title, Trigger (PuzzleSolved, ItemCollected, NoteRead, InteractionPerformed or Manual), Target Id (the puzzle's Persistent Id, the Item Id…) and Prerequisites.
3. Create an Objective Set with the objectives and the Completion Ending. Optionally set Next Room Scene and Next Room Spawn Id to move to another room.
4. Run Escape Room Framework > Create > Flow > Objective Manager and assign the Objective Set.
5. Simple alternative: Escape Room Framework > Create > Flow > Game End Trigger with an Ending. Enable Activate On Player Enter or call Trigger from the last puzzle's On Solved.

**How to test it**

- The game doesn't end before the last objective. On the end screen, Retry restarts the room.

**Watch out for**

- Avoid circular prerequisites.

<a id="hazard"></a>
### Timer and moving hazard

Menu: `Escape Room Framework > Create > Flow > Game Over Timer (HUD) · Moving Hazard (Any Direction)` · Museum room 12

**Steps**

1. Timer: Escape Room Framework > Create > Flow > Game Over Timer (HUD). In the window choose duration, auto start, HUD visibility and label.
2. Hazard: Escape Room Framework > Create > Flow > Moving Hazard (Any Direction). Choose direction, distance, duration and size. Then move StartPoint and EndPoint freely (ceiling, wall, floor, platform or water).
3. To start them: a Generic Trigger whose On Interact Event calls GameOverTimer.StartTimer or MovingHazard.StartHazard, or a Escape Room Framework > Create > Triggers > Event Trigger Zone with On Entered.
4. To stop them on solve: in the puzzle's On Solved, call StopTimer or StopHazard.
5. Assign a defeat Ending for a custom message.

**How to test it**

- Pausing freezes time. When it runs out, the results screen offers Retry.

**Watch out for**


<a id="rooms"></a>
### Changing room or scene

Menu: `RoomPortal + RoomSpawnPoint`

**Steps**

1. In the destination scene, create an empty object and add RoomSpawnPoint with a Spawn Id (for example entrance).
2. In the origin scene, create an object with a collider and add RoomPortal: Target Scene, Target Spawn Id and Load Mode (Single or Additive). It can be locked (Is Locked) or need an item (Required Item Id).
3. Add both scenes to File > Build Profiles.
4. A puzzle can open the portal with Link Selected Objects To Puzzle (calls RoomPortal.Unlock).

**How to test it**

- In Single mode, inventory and state travel with the player. In Additive mode scenes coexist: don't duplicate the GameManager or player.

**Watch out for**

