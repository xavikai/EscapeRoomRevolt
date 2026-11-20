# Step-by-step recipes

**Language:** [català](RECEPTES.md) · [castellano](RECETAS.md) · [English](RECIPES.md)

The same recipes have progress checkboxes in the [interactive guide](https://github.com/xavikai/EscapeRoomRevolt/releases/download/escaperoom-v0.2.2/guia-escape-room-0.2.2.html), under *Step-by-step recipes*.

## Before you start in Unity

1. Open your scene in Edit mode. If `Play` is active, click it again before creating or configuring anything. Open missing panels from `Window > General > Hierarchy`, `Project` and `Inspector`.
2. `Hierarchy` holds scene objects. Click one to see its components in `Inspector`. The puzzle root is the parent object that holds the controller. Click its triangle to see its children. After creating a puzzle, press `F` with the pointer over `Scene` to frame the selection.
3. `Project` holds files and assets. Create yours in a folder under `Assets`. To assign a reference, keep the receiving object selected in `Hierarchy` and drag from `Project` or `Hierarchy` onto the `None` field in `Inspector`. Clicking the source instead of dragging changes the selection.
4. For a PC test, the scene needs a floor with a collider, a `GameManager` and a player. If needed, create the manager and player using `Escape Room Framework > Setup > Instantiate Game Manager` and `Escape Room Framework > Setup > Instantiate PC Player`. Place the player above the floor, near the puzzle. Use the existing objects if the scene already has them.
5. When removing elements, check the controller list and update the labels, hints and solution. Start a new game to test the setup. To experiment, duplicate your test scene in `Project` before editing it.

- Puzzles: [Code keypad](#code) · [Sequence and melody](#seq) · [Levers and switches (StatePuzzle)](#state) · [Number wheels (lock)](#wheels) · [Pipes](#pipe) · [Sliding puzzle](#slide) · [Linked lights](#lights) · [Throw at targets](#throw) · [Piece placement](#place) · [Chained puzzles](#multi)
- Room systems: [Key that opens a door](#key) · [Items, inventory, combining and examine](#items) · [Doors, drawers and cabinets](#doors) · [Hints and narration](#hints) · [Objectives and game ending](#objectives) · [Timer and moving hazard](#hazard) · [Changing room or scene](#rooms)

## Puzzles

<a id="code"></a>
### Code keypad

Menu: `Escape Room Framework > Create > Puzzles > Keypad Panel` · Museum room 2

**Steps**

1. From the top menu run `Escape Room Framework > Create > Puzzles > Keypad Panel`. Select `NewKeypad_Logic` in `Hierarchy` and move the whole object onto the wall using `Move`. The buttons are already wired.
2. On `CodePanelPuzzle` in `Inspector`, set `Correct Code` to `3142` and `Max Code Length` to `4`. Leave `Auto Check When Full` on and `Randomize Code` off. For a shorter code, change both fields, for example to `42` and `2`. Keep the keypad buttons.
3. To give a clue, create a note with `Escape Room Framework > Create > Interactables > Note`. Select `NewNote_Logic` in `Hierarchy` and write the clue in `Note Content` under `InteractableNote` in `Inspector`. Place the note where the player can read it.
4. In `Project`, open your folder under `Assets`. Right-click an empty spot and choose `Create > Escape Room Framework > Puzzles > Puzzle Definition`. Name the asset `Def_sala1_puzle` or give it a recognizable name.
5. Select the definition in `Project`. In `Inspector`, enter a unique `Persistent Id`, such as `sala1_teclat` for the room 1 keypad. Fill in `Display Name` and `Objective`. Select the puzzle root in `Hierarchy` and drag the asset from `Project` onto `Definition` on the puzzle component in `Inspector`.
6. To add hints, right-click in the same `Project` folder and choose `Create > Escape Room Framework > Hints > Hint Data`. Select the `Hint Data` and add text to its `Hints` list. Select the `Puzzle Definition` and drag the `Hint Data` from `Project` onto the definition’s `Hints` field.
7. To open a door when the puzzle is solved, create one from the top menu `Escape Room Framework > Create > Interactables > Door`. In `Hierarchy`, select `NewDoor_Logic` and tick `Is Locked` on its `Door` component in `Inspector`. Place the door in the scene.
8. In `Hierarchy`, select the puzzle root. Hold `Ctrl` on Windows or `Cmd` on Mac and click `NewDoor_Logic` so both are selected. Run `Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle`. Select just the root again and check `On Solved` contains `Door.Unlock` and `Door.ForceOpen`. Save with `Ctrl+S` or `Cmd+S` and enter `Play`.

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

1. Run `Escape Room Framework > Create > Puzzles > Sequence Puzzle`. Expand `NewSequencePuzzle` in `Hierarchy` to see the three buttons. Select the root and expand `SequencePuzzle > Correct Sequence` in `Inspector`. The default order is `red`, `green`, `blue`.
2. To add a button, duplicate a whole button in `Hierarchy` and edit `Step Id` on its `SequenceStepButton`. Add the same text to `Correct Sequence` on the root. To remove the green button, remove every `green` entry and the `greenButton_Logic` object from `Hierarchy`. To shorten just the sequence, remove steps and keep the buttons as options. Keep at least one step.
3. For a melody, select each button in `Hierarchy`, use `Add Component` to add `Audio Source`, drag its sound from `Project` into the clip field and untick `Play On Awake`. In the button’s `On Interact Event`, add an entry with `+`, drag the button onto the entry and choose `AudioSource > Play`. Keep the entry that submits the puzzle step.
4. To play the reference melody, add `MelodyPlayer` to the root using `Add Component`. In `Notes`, add one entry per step and drag in the corresponding button, in order. Create a `Generic Trigger` from `Create > Interactables`. In its `On Interact Event`, click `+`, drag in `NewSequencePuzzle` from `Hierarchy` and choose `MelodyPlayer > Play`. Update `Notes` when you remove steps.
5. In `Project`, open your folder under `Assets`. Right-click an empty spot and choose `Create > Escape Room Framework > Puzzles > Puzzle Definition`. Name the asset `Def_sala1_puzle` or give it a recognizable name.
6. Select the definition in `Project`. In `Inspector`, enter a unique `Persistent Id`, such as `sala1_teclat` for the room 1 keypad. Fill in `Display Name` and `Objective`. Select the puzzle root in `Hierarchy` and drag the asset from `Project` onto `Definition` on the puzzle component in `Inspector`.
7. To add hints, right-click in the same `Project` folder and choose `Create > Escape Room Framework > Hints > Hint Data`. Select the `Hint Data` and add text to its `Hints` list. Select the `Puzzle Definition` and drag the `Hint Data` from `Project` onto the definition’s `Hints` field.
8. To open a door when the puzzle is solved, create one from the top menu `Escape Room Framework > Create > Interactables > Door`. In `Hierarchy`, select `NewDoor_Logic` and tick `Is Locked` on its `Door` component in `Inspector`. Place the door in the scene.
9. In `Hierarchy`, select the puzzle root. Hold `Ctrl` on Windows or `Cmd` on Mac and click `NewDoor_Logic` so both are selected. Run `Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle`. Select just the root again and check `On Solved` contains `Door.Unlock` and `Door.ForceOpen`. Save with `Ctrl+S` or `Cmd+S` and enter `Play`.

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

1. Run `Escape Room Framework > Create > Puzzles > State Puzzle`. Expand `NewStatePuzzle` in `Hierarchy`. Click `Lever1_Logic` and expand `SteppedPositioner > Positions` in `Inspector`. `Element 0` is the first position. `Starting Index` sets where the lever starts.
2. Select the root and open `StatePuzzle > Conditions` in `Inspector`. Each row has `Positioner` and `Required Index`. To add a lever, duplicate its `_Logic` object, click `+` in `Conditions` and drag the new lever from `Hierarchy` into the new row’s `Positioner` field. Set the required position.
3. To keep two levers, select the `Conditions` row that references `Lever3_Logic` and click `–`. Then delete `Lever3_Logic` from `Hierarchy`. If you reduce a lever’s positions, check `Starting Index` and `Required Index` still fit the list. With two positions they can be `0` or `1`.
4. In `Project`, open your folder under `Assets`. Right-click an empty spot and choose `Create > Escape Room Framework > Puzzles > Puzzle Definition`. Name the asset `Def_sala1_puzle` or give it a recognizable name.
5. Select the definition in `Project`. In `Inspector`, enter a unique `Persistent Id`, such as `sala1_teclat` for the room 1 keypad. Fill in `Display Name` and `Objective`. Select the puzzle root in `Hierarchy` and drag the asset from `Project` onto `Definition` on the puzzle component in `Inspector`.
6. To add hints, right-click in the same `Project` folder and choose `Create > Escape Room Framework > Hints > Hint Data`. Select the `Hint Data` and add text to its `Hints` list. Select the `Puzzle Definition` and drag the `Hint Data` from `Project` onto the definition’s `Hints` field.
7. To open a door when the puzzle is solved, create one from the top menu `Escape Room Framework > Create > Interactables > Door`. In `Hierarchy`, select `NewDoor_Logic` and tick `Is Locked` on its `Door` component in `Inspector`. Place the door in the scene.
8. In `Hierarchy`, select the puzzle root. Hold `Ctrl` on Windows or `Cmd` on Mac and click `NewDoor_Logic` so both are selected. Run `Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle`. Select just the root again and check `On Solved` contains `Door.Unlock` and `Door.ForceOpen`. Save with `Ctrl+S` or `Cmd+S` and enter `Play`.

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

1. Run `Escape Room Framework > Create > Puzzles > Number Wheels Puzzle`. In the window, choose 2–8 wheels, enter a digit for each and click `Create puzzle`. `NumberWheelsPuzzle` appears in `Hierarchy` with its camera and controls already wired.
2. To add or remove wheels, select the root in `Hierarchy`. In `Inspector > NumberWheelsPuzzleAuthoring`, change `Wheel Count`, review the digits and click `Rebuild wheels and layout`. To go from four wheels to three, set `Wheel Count = 3` and check all three digits before rebuilding. This rebuilds the board and its conditions. Update the code clue.
3. In `Project`, open your folder under `Assets`. Right-click an empty spot and choose `Create > Escape Room Framework > Puzzles > Puzzle Definition`. Name the asset `Def_sala1_puzle` or give it a recognizable name.
4. Select the definition in `Project`. In `Inspector`, enter a unique `Persistent Id`, such as `sala1_teclat` for the room 1 keypad. Fill in `Display Name` and `Objective`. Select the puzzle root in `Hierarchy` and drag the asset from `Project` onto `Definition` on the puzzle component in `Inspector`.
5. To add hints, right-click in the same `Project` folder and choose `Create > Escape Room Framework > Hints > Hint Data`. Select the `Hint Data` and add text to its `Hints` list. Select the `Puzzle Definition` and drag the `Hint Data` from `Project` onto the definition’s `Hints` field.
6. To open a door when the puzzle is solved, create one from the top menu `Escape Room Framework > Create > Interactables > Door`. In `Hierarchy`, select `NewDoor_Logic` and tick `Is Locked` on its `Door` component in `Inspector`. Place the door in the scene.
7. In `Hierarchy`, select the puzzle root. Hold `Ctrl` on Windows or `Cmd` on Mac and click `NewDoor_Logic` so both are selected. Run `Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle`. Select just the root again and check `On Solved` contains `Door.Unlock` and `Door.ForceOpen`. Save with `Ctrl+S` or `Cmd+S` and enter `Play`.

**How to test it**

- On PC: E enters the close-up view, click ▲/▼, right click leaves. In VR: physical ▲/▼ buttons with the trigger.
- Test it in Play: save with F5 halfway through and once solved, then load with F9. The state must match and the door must stay open.

**Watch out for**

- After changing the wheel count, check the combination is still the one you want.

<a id="pipe"></a>
### Pipes

Menu: `Escape Room Framework > Create > Puzzles > Pipe Puzzle` · Museum room 10

**Steps**

1. **Understand how it works (the logic controller + the 3D pieces).** Unlike the keypad or number wheels, `Escape Room Framework > Create > Puzzles > Pipe Puzzle` creates **only the puzzle brain** (`NewPipePuzzle` with the `PipePuzzle` component). You build the 3D pieces that the player sees and clicks separately in the scene and link them by name (`Tile Id`). The puzzle uses an imaginary **2D grid** (like a table or spreadsheet) and checks whether water can flow from the starting cell (`Source Tile Id`) to the ending cell (`Sink Tile Id`).
2. **What `Row` and `Column` mean and how the grid is oriented.** The puzzle does not check Unity’s 3D X/Y/Z positions to see which pieces touch: **it only checks the `Row` and `Column` numbers** in the `Tiles` list:
   - **`Row` (vertical, top to bottom):** `Row = 0` is the top row (`North`), `Row = 1` is the row below it (`South`), `Row = 2` is one row further down...
   - **`Column` (horizontal, left to right):** `Column = 0` is the leftmost column (`West`), `Column = 1` is the next column to the right (`East`), `Column = 2` is one more to the right...
   - **When do two pieces connect?** Two pieces are neighbors if they share a row and have consecutive columns (e.g. `(Row 0, Column 0)` and `(Row 0, Column 1)`), or share a column and have consecutive rows (e.g. `(Row 0, Column 1)` and `(Row 1, Column 1)`). For water to flow between two neighboring cells, **both must point at each other with opposite openings**: if the left piece `(0,0)` opens `East` (right), the right piece `(0,1)` must open `West` (left); if the top piece `(0,1)` opens `South` (down), the bottom piece `(1,1)` must open `North` (up).
3. **How `Open Sides` (piece shape) and `Starting Rotation Steps` (initial turn) work.** Each entry in `PipePuzzle > Tiles` has five fields:
   - **`Tile Id`**: unique name for the piece (e.g. `pipe_a`). Must match the `Tile Id` on its 3D button.
   - **`Row` and `Column`**: its grid cell (do not give two pieces the same `Row`/`Column` pair).
   - **`Open Sides`**: which sides have openings **at its base rotation (0°, step 0)**. In Unity’s dropdown you can tick multiple flags at once:
     - **Straight pipe**: 2 opposite sides, `North | South` (vertical `│`) or `East | West` (horizontal `─`).
     - **Elbow (90° corner)**: 2 adjacent sides, such as `North | East` (`└`), `East | South` (`┌`), `South | West` (`┐`), or `West | North` (`┘`).
     - **T-junction**: 3 sides, such as `North | East | West` (`┴`). **Cross**: all 4 sides (`Everything`). **Cap / endpoint**: 1 side (`East`).
   - **`Starting Rotation Steps` (`0`, `1`, `2`, or `3`)**: how many 90° clockwise quarter-turns the piece starts with. Each player click adds `+1` (wrapping from `3` back to `0`) and rotates openings clockwise: **`North` → `East` → `South` → `West` → `North`**. *(Example: if `Open Sides = East` and `Starting Rotation Steps = 2`, the piece starts turned 180° facing `West`; clicking it twice returns it to `0` facing `East`).*
4. **Create the controller and inspect the 2 sample pieces.** Run `Escape Room Framework > Create > Puzzles > Pipe Puzzle`. In `Hierarchy`, select `NewPipePuzzle` and look at `PipePuzzle` in `Inspector`. It comes preconfigured with 2 sample tiles in `Tiles`:
   - `Element 0`: `Tile Id = pipe_a`, `Row = 0`, `Column = 0` (left), `Open Sides = East` (points right at step 0), `Starting Rotation Steps = 2` (starts turned 180°, pointing left).
   - `Element 1`: `Tile Id = pipe_b`, `Row = 0`, `Column = 1` (right), `Open Sides = West` (points left at step 0), `Starting Rotation Steps = 2` (starts turned 180°, pointing right).
   - `Source Tile Id = pipe_a` (water start) and `Sink Tile Id = pipe_b` (water goal). When both reach step `0`, `pipe_a` (`East`) and `pipe_b` (`West`) face each other and the puzzle solves.
5. **Build the first 3D piece (`Tram A`) and wire it to the controller.** From the top menu run `Escape Room Framework > Create > Interactables > Generic Trigger`. Select `NewTrigger_Logic` in `Hierarchy`, rename it `Tram A`, and place it on your wall or table:
   - With `Tram A` selected, click `Add Component` in `Inspector` and add **`PipeTileButton`**.
   - On `PipeTileButton`, drag `NewPipePuzzle` from `Hierarchy` into **`Puzzle`**, and type **`pipe_a`** into **`Tile Id`**.
   - On `InteractableTrigger` on the same `Tram A`, expand **`On Interact Event`**, click **`+`**, drag `Tram A` itself into the object slot, and under `No Function` choose **`PipeTileButton > Rotate ()`**.
6. **Make the 3D piece rotate visually when clicked (`SteppedPositioner`).** So the 3D model on `Tram A` turns 90° on each click in sync with the puzzle:
   - With `Tram A` selected, click `Add Component` and add **`SteppedPositioner`**. Keep `Movement Type = Rotate`.
   - Expand **`Positions`** and create **4 entries** (`Element 0` to `Element 3`, matching steps `0`, `1`, `2`, and `3`): for a vertical wall board use rotations `(0, 0, 0)`, `(0, 0, -90)`, `(0, 0, -180)`, and `(0, 0, -270)`; for a horizontal table board use the Y axis: `(0, 0, 0)`, `(0, 90, 0)`, `(0, 180, 0)`, and `(0, 270, 0)`.
   - Drag the visual child (`NewTrigger_Visuals`, or your pipe 3D mesh inside it) into **`Visual Transform`** on `SteppedPositioner` (so the mesh turns while the collider stays put). **Important:** orient the child 3D mesh so that at `0°` (`Element 0`) its openings visibly match the directions ticked in `Open Sides`.
   - On **`PipeTileButton`** on `Tram A`, drag the `SteppedPositioner` into **`Visual Positioner`**. This automatically snaps the 3D model to its starting rotation when Play begins or a save loads.
   - **Create `Tram B` by duplicating:** duplicate `Tram A` in `Hierarchy` (`Ctrl+D`), rename it `Tram B`, move it to the right of `Tram A`, change its `PipeTileButton > Tile Id` to **`pipe_b`**, and check that its `Visual Transform` points to `Tram B`’s own visual child.
7. **Practical example to expand the puzzle (4-piece 2×2 grid).** To build a 4-piece 2×2 grid route (from top-left to bottom-right), set these 4 entries in `PipePuzzle > Tiles` and duplicate your 3D segments so you have 4 pieces (`pipe_a`, `pipe_b`, `pipe_c`, `pipe_d`) arranged in a 2×2 square in the scene:
   - **`pipe_a` (Top-Left, start)**: `Row = 0, Column = 0` · `Open Sides = East` (exits right) · `Starting Rotation Steps = 1`.
   - **`pipe_b` (Top-Right, elbow)**: `Row = 0, Column = 1` · `Open Sides = West | South` (enters from left and goes down) · `Starting Rotation Steps = 2`.
   - **`pipe_c` (Bottom-Left, optional decoy)**: `Row = 1, Column = 0` · `Open Sides = North | East` · `Starting Rotation Steps = 0`.
   - **`pipe_d` (Bottom-Right, goal)**: `Row = 1, Column = 1` · `Open Sides = North` (receives water coming down from `pipe_b`) · `Starting Rotation Steps = 3`.
   - Set **`Source Tile Id = pipe_a`** and **`Sink Tile Id = pipe_d`**. Once `pipe_a`, `pipe_b`, and `pipe_d` are rotated to step `0`, the path `(0,0) → (0,1) → (1,1)` connects and solves the puzzle.
8. In `Project`, open your folder under `Assets`. Right-click an empty spot and choose `Create > Escape Room Framework > Puzzles > Puzzle Definition`. Name the asset `Def_sala1_puzle` or give it a recognizable name.
9. Select the definition in `Project`. In `Inspector`, enter a unique `Persistent Id`, such as `sala1_teclat` for the room 1 keypad. Fill in `Display Name` and `Objective`. Select the puzzle root in `Hierarchy` and drag the asset from `Project` onto `Definition` on the puzzle component in `Inspector`.
10. To add hints, right-click in the same `Project` folder and choose `Create > Escape Room Framework > Hints > Hint Data`. Select the `Hint Data` and add text to its `Hints` list. Select the `Puzzle Definition` and drag the `Hint Data` from `Project` onto the definition’s `Hints` field.
11. To open a door when the puzzle is solved, create one from the top menu `Escape Room Framework > Create > Interactables > Door`. In `Hierarchy`, select `NewDoor_Logic` and tick `Is Locked` on its `Door` component in `Inspector`. Place the door in the scene.
12. In `Hierarchy`, select the puzzle root. Hold `Ctrl` on Windows or `Cmd` on Mac and click `NewDoor_Logic` so both are selected. Run `Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle`. Select just the root again and check `On Solved` contains `Door.Unlock` and `Door.ForceOpen`. Save with `Ctrl+S` or `Cmd+S` and enter `Play`.

**How to test it**

- In the initial 2-piece example (`pipe_a` and `pipe_b`), enter Play and click each piece twice: on reaching step 0 they connect and open the door. Once solved, segments stop turning.
- Test it in Play: save with F5 halfway through and once solved, then load with F9. The state must match and the door must stay open.

**Watch out for**

- If pieces turn but the puzzle never solves, check 3 things: 1) `Source Tile Id` and `Sink Tile Id` exist in `Tiles`, 2) neighboring cells have adjacent `Row`/`Column` numbers and opposite openings (`East` meets `West`, `North` meets `South`), and 3) the 3D model at step 0 (`Element 0` of `SteppedPositioner`) visually points toward the sides ticked in `Open Sides`.
- Randomize Rotations shuffles starting rotations each playthrough and ensures it never starts already solved.

<a id="slide"></a>
### Sliding puzzle

Menu: `Escape Room Framework > Create > Puzzles > Sliding Puzzle` · Museum room 8

**Steps**

1. Run `Escape Room Framework > Create > Puzzles > Sliding Puzzle`. In `Hierarchy`, expand `NewSlidingPuzzle` and select `Board`. Import an image by dragging its file into your `Assets` folder in `Project`. With `Board` selected, drag the image from `Project` onto `SlidingBoardView > Source Image` in `Inspector`. Click `Re-apply layout and image` if it needs refreshing.
2. To shrink the 3×3 board to 2×2, select `NewSlidingPuzzle` and set `Columns = 2`, `Rows = 2` and `Hole Cell` to `X = 1`, `Y = 1`. Select `Board` and click `Rebuild board`. This gives you three tiles and an empty space. Use the same button when enlarging the grid. If you use `Custom Target Order`, update it for the new size.
3. In `Project`, open your folder under `Assets`. Right-click an empty spot and choose `Create > Escape Room Framework > Puzzles > Puzzle Definition`. Name the asset `Def_sala1_puzle` or give it a recognizable name.
4. Select the definition in `Project`. In `Inspector`, enter a unique `Persistent Id`, such as `sala1_teclat` for the room 1 keypad. Fill in `Display Name` and `Objective`. Select the puzzle root in `Hierarchy` and drag the asset from `Project` onto `Definition` on the puzzle component in `Inspector`.
5. To add hints, right-click in the same `Project` folder and choose `Create > Escape Room Framework > Hints > Hint Data`. Select the `Hint Data` and add text to its `Hints` list. Select the `Puzzle Definition` and drag the `Hint Data` from `Project` onto the definition’s `Hints` field.
6. To open a door when the puzzle is solved, create one from the top menu `Escape Room Framework > Create > Interactables > Door`. In `Hierarchy`, select `NewDoor_Logic` and tick `Is Locked` on its `Door` component in `Inspector`. Place the door in the scene.
7. In `Hierarchy`, select the puzzle root. Hold `Ctrl` on Windows or `Cmd` on Mac and click `NewDoor_Logic` so both are selected. Run `Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle`. Select just the root again and check `On Solved` contains `Door.Unlock` and `Door.ForceOpen`. Save with `Ctrl+S` or `Cmd+S` and enter `Play`.

**How to test it**

- Only a tile next to the hole moves; others give a small nudge.
- Test it in Play: save with F5 halfway through and once solved, then load with F9. The state must match and the door must stay open.

**Watch out for**


<a id="lights"></a>
### Linked lights

Menu: `Escape Room Framework > Create > Puzzles > Linked Lights Puzzle` · Museum room 14

**Steps**

1. Run `Escape Room Framework > Create > Puzzles > Linked Lights Puzzle`. Select `LinkedLights_Kit` in `Hierarchy`. Expand it to find the five buttons and `ResetButton`. Select the root and open `LinkedLightsPuzzle > Nodes` in `Inspector`. Button labels start at 1 and indices at 0.
2. To add a light, duplicate a button in `Hierarchy`, give it the new `Index` on `LinkedLightButton` and add a row to `Nodes`. Select the root and drag the duplicate button into the new row’s `Indicator` field. Review `Connections` on the rows you want linked to it.
3. To go from five lights to four, remove `Element 4` from `Nodes` and delete `LightButton_5` and `Number_5` in `Hierarchy`. In `Element 3 > Connections`, keep `2, 3`. If you remove a middle light, update changed `Index` values and `Connections`. For this four-light example, turn every `Initially On` off, turn every `Target On` on, and test buttons 1 and 4.
4. If you are on 0.2.1 and the menu is missing, open `ShowcaseMuseum` from `Project > Assets > _EscapeRoomTemplate > Scenes`. In `Hierarchy`, expand `Room14_LinkedLights` and drag `LinkedLights_Kit` into your own `Project` folder to make a prefab. Reopen your scene and drag in the prefab. Remove any demo references from `On Solved` and `ResetButton`. Keep `LinkedLightsPuzzle.ResetPuzzle` on the button. Assign your own definition.
5. In `Project`, open your folder under `Assets`. Right-click an empty spot and choose `Create > Escape Room Framework > Puzzles > Puzzle Definition`. Name the asset `Def_sala1_puzle` or give it a recognizable name.
6. Select the definition in `Project`. In `Inspector`, enter a unique `Persistent Id`, such as `sala1_teclat` for the room 1 keypad. Fill in `Display Name` and `Objective`. Select the puzzle root in `Hierarchy` and drag the asset from `Project` onto `Definition` on the puzzle component in `Inspector`.
7. To add hints, right-click in the same `Project` folder and choose `Create > Escape Room Framework > Hints > Hint Data`. Select the `Hint Data` and add text to its `Hints` list. Select the `Puzzle Definition` and drag the `Hint Data` from `Project` onto the definition’s `Hints` field.
8. To open a door when the puzzle is solved, create one from the top menu `Escape Room Framework > Create > Interactables > Door`. In `Hierarchy`, select `NewDoor_Logic` and tick `Is Locked` on its `Door` component in `Inspector`. Place the door in the scene.
9. In `Hierarchy`, select the puzzle root. Hold `Ctrl` on Windows or `Cmd` on Mac and click `NewDoor_Logic` so both are selected. Run `Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle`. Select just the root again and check `On Solved` contains `Door.Unlock` and `Door.ForceOpen`. Save with `Ctrl+S` or `Cmd+S` and enter `Play`.

**How to test it**

- With the sample setup it solves by pressing 1, 3 and 5. The reset button restores the initial state.
- Test it in Play: save with F5 halfway through and once solved, then load with F9. The state must match and the door must stay open.

**Watch out for**

- Make sure it is solvable: start from the goal and apply a few presses to get the initial state.

<a id="throw"></a>
### Throw at targets

Menu: `Escape Room Framework > Create > Puzzles > Throw Puzzle` · Museum room 6

**Steps**

1. Run `Escape Room Framework > Create > Puzzles > Throw Puzzle`. In `Hierarchy`, expand `NewThrowPuzzle`. You will see `Target1_Logic`, `Target2_Logic` and `Target3_Logic`. Each target has a `ThrowTarget` and a solid collider.
2. To keep just one target, select `Target2_Logic` and `Target3_Logic` in `Hierarchy` and remove them with `Delete`. Keep `Target1_Logic` and its visual child. Select `NewThrowPuzzle` again. In `Inspector > ThrowPuzzle > Target Ids`, set the list size to `1` or remove the two extra rows with `–`. Leave only `Element 0 = target_1`.
3. Select `Target1_Logic` and check in `Inspector` that `ThrowTarget > Target Id` reads `target_1`. If `Puzzle` is empty, drag in `NewThrowPuzzle` from `Hierarchy`. To add a target later, duplicate the `_Logic` object, change its `Target Id` and add the same ID to `Target Ids` on the root.
4. Create a throwable object with `Escape Room Framework > Create > Interactables > Physics Grabbable`. Select `NewGrabbable_Logic` in `Hierarchy`. In `Inspector`, check `Can Be Thrown` on `PhysicsGrabbable` and that the collider is enabled with `Is Trigger` off and `Is Kinematic` is off on the `Rigidbody`. Place it within reach. In `Play` on PC, look at it and press `E`, then aim at the target and throw with left click. With one target, an impact above `Min Impact Speed` should solve the puzzle.
5. In `Project`, open your folder under `Assets`. Right-click an empty spot and choose `Create > Escape Room Framework > Puzzles > Puzzle Definition`. Name the asset `Def_sala1_puzle` or give it a recognizable name.
6. Select the definition in `Project`. In `Inspector`, enter a unique `Persistent Id`, such as `sala1_teclat` for the room 1 keypad. Fill in `Display Name` and `Objective`. Select the puzzle root in `Hierarchy` and drag the asset from `Project` onto `Definition` on the puzzle component in `Inspector`.
7. To add hints, right-click in the same `Project` folder and choose `Create > Escape Room Framework > Hints > Hint Data`. Select the `Hint Data` and add text to its `Hints` list. Select the `Puzzle Definition` and drag the `Hint Data` from `Project` onto the definition’s `Hints` field.
8. To open a door when the puzzle is solved, create one from the top menu `Escape Room Framework > Create > Interactables > Door`. In `Hierarchy`, select `NewDoor_Logic` and tick `Is Locked` on its `Door` component in `Inspector`. Place the door in the scene.
9. In `Hierarchy`, select the puzzle root. Hold `Ctrl` on Windows or `Cmd` on Mac and click `NewDoor_Logic` so both are selected. Run `Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle`. Select just the root again and check `On Solved` contains `Door.Unlock` and `Door.ForceOpen`. Save with `Ctrl+S` or `Cmd+S` and enter `Play`.

**How to test it**

- On PC: E to grab, left click to throw, Q to drop. In VR: grab with grip and release with a throwing motion.
- Test it in Play: save with F5 halfway through and once solved, then load with F9. The state must match and the door must stay open.

**Optional · Make one or more 3D knives stick to the target (without touching the template)**

1. **Set up the knife (`PhysicsGrabbable` + `GrabbablePiece`).** On your throwable object (`Ganivet_Logic`), add `GrabbablePiece` via `Add Component`. Besides enabling locking, `GrabbablePiece` has `Return When Lost` enabled by default: if a throw misses and falls out of reach, the knife returns to its starting spot automatically.
2. **Mark the final position and angle (`PuntClavat`).** Right-click `Target1_Logic` in `Hierarchy` → `Create Empty`, name it `PuntClavat`, and position/rotate it with the tip pointing into the target.
3. **Wire the event (`GrabbablePiece.LockInPlace`).** For 1 target and 1 knife, in `ThrowPuzzle > On Solved` click `+`, drag `Ganivet_Logic`, choose `GrabbablePiece > LockInPlace (Transform)`, and drag `PuntClavat` into the `Transform` field. `LockInPlace` freezes the `Rigidbody` (`isKinematic = true`), parents it to the target, and disables the outline and PC/VR grabbing.
4. **Multiple targets and knives without code (`MultiStagePuzzle`).** For 3 targets and 3 knives without writing code, group 3 single-target `ThrowPuzzle` controllers inside a `MultiStagePuzzle` (with `Require Order` unticked) and wire each child’s `On Solved` to its knife’s `LockInPlace`.
5. **Multiple targets with any knife sticking on impact (micro-script in your folder).** To make any thrown knife stick immediately to whichever target it hits, add a small script in your own folder (`Assets/MyGame/StickKnifeOnTarget.cs`) on each target that checks `collision.relativeVelocity.magnitude >= _minImpactSpeed` in `OnCollisionEnter` and calls `piece.LockInPlace(_snapPoint)`.

**Watch out for**


<a id="place"></a>
### Piece placement

Menu: `Escape Room Framework > Create > Puzzles > Placement Puzzle` · Museum room 7

**Steps**

1. Run `Escape Room Framework > Create > Puzzles > Placement Puzzle`. Expand `NewPlacementPuzzle` in `Hierarchy`. `PieceA_Logic` and `PieceB_Logic` are the pieces. `SocketA_Logic`, `SocketB_Logic` and `SocketC_Logic` are the sockets. Select the root and open `PlacementPuzzle > Rules` in `Inspector`.
2. To add a pair, duplicate a whole piece and socket in `Hierarchy`. On the new piece’s `GrabbablePiece`, set `Piece Id = piece_c`. On the new socket’s `PieceSocketReceiver`, set `Socket Id = socket_d`. If `Puzzle` is empty, drag in `NewPlacementPuzzle`. On the root, click `+` in `Rules` and enter `piece_c` in `Piece Id` and `socket_d` in `Correct Socket Id`.
3. To keep piece A, remove the `piece_b → socket_b` row from `Rules`. In `Hierarchy`, delete `PieceB_Logic` and `SocketB_Logic`. To remove the decoy as well, delete `SocketC_Logic`. It has no rule, so the list stays the same. Keep `piece_a → socket_a` and place both objects within reach.
4. In `Project`, open your folder under `Assets`. Right-click an empty spot and choose `Create > Escape Room Framework > Puzzles > Puzzle Definition`. Name the asset `Def_sala1_puzle` or give it a recognizable name.
5. Select the definition in `Project`. In `Inspector`, enter a unique `Persistent Id`, such as `sala1_teclat` for the room 1 keypad. Fill in `Display Name` and `Objective`. Select the puzzle root in `Hierarchy` and drag the asset from `Project` onto `Definition` on the puzzle component in `Inspector`.
6. To add hints, right-click in the same `Project` folder and choose `Create > Escape Room Framework > Hints > Hint Data`. Select the `Hint Data` and add text to its `Hints` list. Select the `Puzzle Definition` and drag the `Hint Data` from `Project` onto the definition’s `Hints` field.
7. To open a door when the puzzle is solved, create one from the top menu `Escape Room Framework > Create > Interactables > Door`. In `Hierarchy`, select `NewDoor_Logic` and tick `Is Locked` on its `Door` component in `Inspector`. Place the door in the scene.
8. In `Hierarchy`, select the puzzle root. Hold `Ctrl` on Windows or `Cmd` on Mac and click `NewDoor_Logic` so both are selected. Run `Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle`. Select just the root again and check `On Solved` contains `Door.Unlock` and `Door.ForceOpen`. Save with `Ctrl+S` or `Cmd+S` and enter `Play`.

**How to test it**

- Try swapped pieces, removing them before solving and loading once solved: pieces stay locked in.
- If you save halfway, loose pieces return to their start on load.

**Watch out for**

- Randomize Mapping changes the correct holder each playthrough: the clue must be dynamic.

<a id="multi"></a>
### Chained puzzles

Menu: `Escape Room Framework > Create > Puzzles > Multi-Stage Puzzle` · Museum room 11

**Steps**

1. Run `Escape Room Framework > Create > Puzzles > Multi-Stage Puzzle`. In `Hierarchy`, expand `MultiStageChainPuzzle` and then `MultiStage_Generated`. It contains a sequence puzzle and a lever puzzle. To edit one, select that stage’s child. To change the order, select the root and open `MultiStagePuzzle > Puzzles` in `Inspector`.
2. To add a stage, create a puzzle with its menu and place it in the scene. On the group root, click `+` in `Puzzles` and enter a distinct `Id`. Drag the new puzzle root from `Hierarchy` into `Puzzle`. Assign the object containing its controls to `Interaction Root`, usually the same root.
3. To remove a stage, remove its row from `Puzzles` and remove its object from `Hierarchy` if you no longer want it in the room. Keep at least one puzzle in the group. With `Require Order` on, stages follow the list order. `Lock Future Puzzles` prevents later controls from responding before their turn. Wire the door to the group root.
4. In `Project`, open your folder under `Assets`. Right-click an empty spot and choose `Create > Escape Room Framework > Puzzles > Puzzle Definition`. Name the asset `Def_sala1_puzle` or give it a recognizable name.
5. Select the definition in `Project`. In `Inspector`, enter a unique `Persistent Id`, such as `sala1_teclat` for the room 1 keypad. Fill in `Display Name` and `Objective`. Select the puzzle root in `Hierarchy` and drag the asset from `Project` onto `Definition` on the puzzle component in `Inspector`.
6. To add hints, right-click in the same `Project` folder and choose `Create > Escape Room Framework > Hints > Hint Data`. Select the `Hint Data` and add text to its `Hints` list. Select the `Puzzle Definition` and drag the `Hint Data` from `Project` onto the definition’s `Hints` field.
7. To open a door when the puzzle is solved, create one from the top menu `Escape Room Framework > Create > Interactables > Door`. In `Hierarchy`, select `NewDoor_Logic` and tick `Is Locked` on its `Door` component in `Inspector`. Place the door in the scene.
8. In `Hierarchy`, select the puzzle root. Hold `Ctrl` on Windows or `Cmd` on Mac and click `NewDoor_Logic` so both are selected. Run `Escape Room Framework > Create > Puzzles > Link Selected Objects To Puzzle`. Select just the root again and check `On Solved` contains `Door.Unlock` and `Door.ForceOpen`. Save with `Ctrl+S` or `Cmd+S` and enter `Play`.

**How to test it**

- The door opens only when every child is solved.
- Test it in Play: save with F5 halfway through and once solved, then load with F9. The state must match and the door must stay open.

**Watch out for**


## Room systems

<a id="key"></a>
### Key that opens a door

Menu: `Create > Escape Room Framework > Inventory > Item` + `Escape Room Framework > Create > Interactables > Pickable Item · Door` · Museum room 1 · 3

**Steps**

1. **Create the key item asset (`InventoryItemData`).** In the `Project` panel, open your folder under `Assets`. Right-click an empty space and choose `Create > Escape Room Framework > Inventory > Item`. Name the asset `Item_OfficeKey`. Select it and, in `Inspector`, type a unique lowercase identifier without spaces in `Item Id`, such as `office_key`. Fill in `Display Name` (for example `Office Key`) and `Description`. If you don't have a custom sprite for `Icon`, you can generate one automatically later via `Escape Room Framework > Maintenance > Generate Missing Item Icons`.
2. **Register the key in the catalog (`DefaultItemCatalog`).** In `Project`, select `Assets/_EscapeRoomTemplate/ScriptableObjects/DefaultItemCatalog.asset` (the catalog used by `InventoryManager` on the `GameManager`). In `Inspector`, expand `Items`, click `+` and drag your `Item_OfficeKey` asset from `Project`. This lets save and load recognize the key.
3. **Place the key in the scene (`Pickable Item`).** From the top menu run `Escape Room Framework > Create > Interactables > Pickable Item`. In `Hierarchy`, select `NewPickableItem_Logic` (you can rename it `OfficeKey_Logic`) and move it where the player should find it (on a desk, on the floor, or inside a drawer). With `OfficeKey_Logic` selected in `Hierarchy`, drag `Item_OfficeKey` from `Project` onto the `Item Data` field of `Pickable Item` in `Inspector`. Leave `Quantity` at `1`.
4. **Create the door and lock it with the key (`Door`).** From the top menu run `Escape Room Framework > Create > Interactables > Door` (or select an existing door, cabinet or drawer). In `Hierarchy`, select `NewDoor_Logic` and inspect its `Door` component in `Inspector`. Tick `Is Locked` and, in `Required Item Id`, type the exact same text you entered in the key's `Item Id` (`office_key`).
5. **Choose how the key is used (`Item Use Policy`).** On the same `Door` component, leave `Item Use Policy` on `Offer Compatible` (default: pressing `E` on the locked door opens a HUD selector with the key to confirm using it, and pressing `E` again opens the door) or set it to `Auto Use Single` so pressing `E` unlocks and opens the door immediately without a menu when the player carries the key. Tick `Consume Required Item` if the key should be removed on use, or untick it if the key opens multiple doors.
6. **Alternative with a separate keyhole or pedestal (`Item Receiver`).** If you prefer a separate keyhole or pedestal that spawns the key's 3D model, run `Escape Room Framework > Create > Interactables > Item Receiver`. Drag `Item_OfficeKey` onto `Required Item` (this field takes the asset reference, not text) and, under `On Item Accepted`, click `+`, drag the door from `Hierarchy` and choose `Door.Unlock` (plus a second row with `Door.ForceOpen` if it should swing open automatically).

**How to test it**

- Enter `Play`, walk up to the door before picking up the key and press `E`: it must stay locked and show the locked prompt.
- Look at the key and press `E` to pick it up (press `I` to verify it is in the inventory). Return to the door and press `E`: with `Offer Compatible`, confirm the key in the selector and press `E` to open; with `Auto Use Single`, it opens right away.
- Save with `F5` after opening the door and load with `F9`: the picked-up key does not respawn and the door stays unlocked and open.

**Watch out for**

- `Required Item Id` on `Door` is a text string and must match the `InventoryItemData`'s `Item Id` **character for character** (including case and underscores), not the asset filename or `Display Name`.
- If you forget to add the asset to `DefaultItemCatalog`, the key will disappear from the inventory when loading a saved game.

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

