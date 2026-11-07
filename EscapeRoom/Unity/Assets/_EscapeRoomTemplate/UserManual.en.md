# User Manual - Escape Room Framework

**Language:** [castellano](UserManual.md) · [català](UserManual.ca.md) · English

**Start here:** [practical guide to PC/VR Escape Rooms](Documentation/ESCAPE_ROOM_QUICKSTART.en.md), the [step-by-step recipes for each mechanic](Documentation/RECIPES.md), saving, VR preparation and the acceptance matrix. See [the September 9, 2026 audit](AUDITORIA_ESCAPE_ROOM_2026-09-09.md) for the current results; the August report is historical.

This guide covers the designer workflow. The architecture and APIs are documented in [PROGRAMMING_GUIDE.md](PROGRAMMING_GUIDE.md). The exhaustive reference, with tutorials, examples and troubleshooting, is in [DOCUMENTACIO_COMPLETA.md](DOCUMENTACIO_COMPLETA.md). The historical room-by-room walkthrough is in [AUDITORIA_ESCAPE_ROOM_2026-08-09.md](AUDITORIA_ESCAPE_ROOM_2026-08-09.md). To learn the template hands-on, open the interactive guide [`docs/guia/index.html`](../../docs/guia/index.html) (Catalan, Spanish and English).

## 1. Framework menu

All supported tools are under `Escape Room Framework`:

- `Configuration`: selects the Escape Room profile, the Survival Horror profile or a custom combination.
- `Setup`: installs safe instances of the Game Manager or player and generates the platform scenes/prefabs.
- `Create`: creates interactables, puzzles, examine hotspots, triggers and flow components without modifying other objects. `Multi-Stage Puzzle` creates a group of two physically separate, visible puzzles; the list accepts as many puzzles as needed and lets them be completed in free or mandatory order before activating a single door or mechanism. `Number Wheels Puzzle` opens a configurator to choose between 2 and 8 wheels and define the combination. It can later be resized from `NumberWheelsPuzzleAuthoring > Rebuild wheels and layout`; the housing, title, conditions, buttons and camera adapt without losing the puzzle definition or events. On PC, the focused view opens and you left-click on ▲/▼; `E` only opens the panel or acts as an alternative interaction. In VR, the equivalent ▲/▼ controls are used with the trigger. In `Create > Flow`, `Moving Hazard (Any Direction)` separately creates a wall, ceiling, floor, platform or volume that moves between two 3D markers, while `Game Over Timer (HUD)` creates an optional time limit shown in the interface. `Pipe Puzzle` still requires its interactive presentation to be completed.
- Both `MovingHazard.StartHazard` and `GameOverTimer.StartTimer` can be connected from the Inspector to a switch (`InteractableTrigger`) or to a walk-through zone (`EventTriggerZone`). The zone supports tag filtering, single-use mode, enter/exit events and re-arming via `ResetZone`.
- `Demo`: opens the sample scenes after offering to save the current changes. `Apply Escape Room Closure Fixes` idempotently reapplies definitions, the Pipe payoff, prompts and semantic naming in `ShowcaseMuseum` and `LockedOffice`.
- `Validation`: checks IDs, dependencies, the active scene and commercial readiness.
- `Maintenance`: previews problems before allowing a repair with Undo.
- `Documentation`: opens this manual, the programming guide, the complete documentation, or locates the UI Toolkit HUD.

The old destructive generators and the automatic package installation are no longer part of the menu.

### Choosing the project genre

- `Configuration/Use Escape Room Profile`: keeps interaction, inventory, puzzles, hints, objectives, endings, Save/Load, PC and VR. Disables and hides the flashlight, battery, stability/sanity and horror events.
- `Configuration/Use Survival Horror Profile`: enables all the common mechanics plus the flashlight, sanity and horror events.
- `Configuration/Use Custom Hybrid Profile`: lets you choose `Flashlight`, `Sanity` and `Horror Events` individually in `GenreFeatureSettings.asset`.

The demo scenes `ShowcaseMuseum` and `ShowcaseMuseumVR` include a local exception for the flashlight, because room 3 demonstrates combining an empty flashlight with batteries. That is why the flashlight works in those scenes even with the `Escape Room` profile; new scenes still follow the profile and do not enable the flashlight unless `Custom Hybrid` is used.

The change takes effect the next time you enter Play. Optional components can remain in scenes and prefabs: the profile prevents them from running or appearing in the UI when they do not apply.

## 2. Playable scene

1. Open or create a scene.
2. Use `Setup/Instantiate Game Manager`.
3. Use `Setup/Instantiate PC Player` or place `Player_VR`.
4. Create interactables from `Create/Interactables` and configure their fields in the Inspector.
5. Run `Validation/Validate Current Scene`.

The visual model of replaceable prefabs lives under a `ModelSocket`. Replace only its visual children to keep colliders, IDs, events and scripting intact.

## 3. Main menu and end of the game

Use `Setup/Create or Update Main Menu Scene` to generate the main menu. It is placed first in Build Settings, unless an enabled `Intro` already exists: in that case the order `Intro → MainMenu` is kept.

`Nueva partida` (New Game) loads the scene specified in `Resources/GameFlowSettings.asset`. In the sample Escape Room profile it should point to `ShowcaseMuseum`; change it explicitly when you start your final game.

To end a playthrough you can:

- create an `Objective Set` and assign it to an `ObjectiveManager`;
- create `Create/Flow/Game End Trigger` and connect it to a puzzle or volume;
- call `GameFlowManager.CompleteGame` or `FailGame` from code.

The end screen lets the player retry, return to the main menu or quit.

## 4. Customizing the menu's look and texts

### Changing colors, fonts and logo

1. In the Project panel, right-click → `Create > Escape Room Framework > Menu Theme Settings`. Give it a name, for example `MiTemaDeMenu`.
2. In the Inspector of the new asset, adjust the colors (panel background, accent, title, buttons) and, if you like, drag an already imported font (`.ttf`/`.otf`) into `Title Font`/`Body Font` and an image into `Logo`.
3. In `MainMenu.unity`, select `MainMenuUI`. In a playable scene, select `MenuUI`, inside the `GameManager`. Both have the `UI Toolkit Menu Controller` component; drag your asset into its `_theme` field.
4. If you have several playable scenes with independent `GameManager` instances, assign the same asset in each one or in the shared prefab.
5. Optional: type your game's name in `Main Menu Title` and the credits (authors, course, licenses) in `Credits Text`. If left empty, the template texts are kept.
6. Enter Play — the menu now uses your palette, typefaces, logo and texts. If nothing is assigned, the menu keeps the template's original design.

If you prefer to edit the stylesheet directly instead of creating an asset, `EscapeRoomMenu.uss` has the most frequently repeated colors as variables at the top of the file (`--color-accent`, `--color-text`...), so changing the base palette means editing a few lines instead of hunting down each individual color.

The player can enable a high-contrast mode from Settings; that mode always takes priority over your theme, so accessibility never depends on visual customization.

### Designing buttons from images

You can use your own images for the button backgrounds. In the current version, `Menu Theme Settings` controls colors, fonts and logo, but button images are assigned from `EscapeRoomMenu.uss`.

In a folder of your own, for example `Assets/UI/Menu/`, prepare one image for each state:

- `ButtonNormal.png`: normal state;
- `ButtonHover.png`: when the mouse hovers over it;
- `ButtonPressed.png`: while it is being pressed;
- optionally, `ButtonDisabled.png`: disabled button.

Recommendations:

- use PNG with transparency when needed;
- keep the same aspect ratio in all variants;
- do not draw the text into the image: the text is generated by the menu and can be changed through the localization catalog;
- import the images as `Sprite (2D and UI)`;
- if the image has a frame whose corners must be preserved when resizing, prepare it for 9-slice.

To assign them:

1. Open `Assets/_EscapeRoomTemplate/UI/Toolkit/EscapeRoomMenu.uss` with UI Builder.
2. Select the `.menu-button` selector and assign the normal image in **Background > Image**.
3. Select or create `.menu-button:hover` and assign the hover image.
4. Select or create `.menu-button:active` and assign the pressed image.
5. If you have buttons that can be disabled, also configure `.menu-button:disabled`.

It is better to assign the images from UI Builder so that Unity writes the asset references correctly. If the button keeps a color underneath the image, set the background color in `Menu Theme Settings` to alpha 0 or use an opaque background in the image itself.

Keep the text separate from the image. That way translations and high-contrast mode will keep working.

### Changing the displayed texts and languages

All texts the player sees go through a single catalog that can be edited without touching code: menus, HUD, interaction prompts (`[E] Abrir armario` — "[E] Open cabinet"), VR panel, subtitles, hints, objectives, item names and descriptions, notes and the 3D signs in the scenes. The catalog includes **Spanish (`es`), English (`en`) and Catalan (`ca`)**; the player chooses the language in Settings.

The rule is simple: **write the texts in Spanish** in the Inspector (prompts, item names, hints, signs) and add their translation to the catalog.

1. Select `Assets/_EscapeRoomTemplate/Resources/DefaultLocalizationCatalog.asset`.
2. Each entry has a key (the exact Spanish text, for example `"Abrir armario"`) and one row per language.
3. For a new text, add an entry whose key is identical to the Inspector text (capitalization, accents and line breaks included) and the rows `es`, `en` and `ca`.
4. To add a language, add rows with its code (`fr`, `it`...). It automatically appears in the language drop-down in Settings.

If a key does not exist, the text is shown as is, so an untranslated text never disappears: it just does not change language. 3D signs (TextMeshPro or TextMesh) are translated automatically if their text is a catalog key (`SceneTextLocalizer`); texts that a script changes at runtime are left untouched.

## 5. Inventory

On PC, the inventory opens with `I`. Storage is no longer limited by the quick bar.

- Select an item to see only the valid actions: read, hold/equip, consume, examine, combine or drop.
- `ACCESO RÁPIDO N` (QUICK ACCESS N) assigns the item to the active quick slot.
- The `1-4` keys, the mouse wheel or the gamepad shoulder buttons change the quick slot.
- When interacting with a lock in `Offer Compatible` mode, the interface shows only valid items. It does not use any of them without confirmation.

Each door or receiver can change its policy to `Selected Only` or `Auto Use Single` from the Inspector.

### Examining an item in 3D

The template lets you inspect in 3D an item that is already in the inventory:

1. Open the inventory with `I`.
2. Select the item.
3. Press `EXAMINAR` (EXAMINE).
4. Drag over the item's image to rotate it.
5. Use the mouse wheel to zoom in or out.
6. Press `ESC` or `CERRAR` (CLOSE) to return to the inventory.

For the button to appear enabled, the `InventoryItemData` must have:

- a `WorldPrefab` assigned;
- `Can Examine` enabled.

The model shown is a temporary visual copy: examining it does not remove or modify the real item in the inventory. If the item has an `ExamineHotspot`, the player can hover the cursor over that area to see a hint and click to reveal it. Hotspots can grant another item, fire an event or show a description, and their state is kept in saved games.

Readable notes (`Is Readable`) use the inventory's text reader and do not need a 3D model. In VR, the same panel is presented as 3D UI and uses the controller's pointer events; even so, VR support remains experimental and must be tested with the final headset.

## 6. Default PC controls

- WASD: movement.
- Mouse: look.
- Left Shift: run.
- Left Ctrl: crouch.
- E: interact or store a held physical object.
- I: inventory.
- F: turn the equipped flashlight on/off.
- R: recharge the flashlight.
- Q: drop a held physical object.
- G: drop equipment.
- H: request a hint.
- Left Alt + A/D: lean in Survival Horror.
- X: look back in Survival Horror.
- V while running forward: slide in Survival Horror.
- Esc: close the current panel or pause.
- F5/F9: quick save/load.

The main controls can be remapped during the game from `Ajustes > Controles` (Settings > Controls); the changes are saved outside the saved games. To modify gamepad or XR bindings, edit `Resources/Input/EscapeRoomInputActions.inputactions`.

## 7. VR preparation

**Experimental**: VR support is functionally complete (rig, hands, haptics, 3D UI, comfort) but has not yet passed QA on a real physical headset — only in the XRI simulator. Do not assume full parity with PC until it has been validated on hardware.

`ShowcaseMuseumVR` contains the VR version of the museum and keeps the same puzzles from rooms 11 and 13. `VRTemplate` is a minimal startup scene and does not contain the museum rooms.

1. Wait for Package Manager to finish importing OpenXR, XR Plug-in Management and XRI.
2. Configure OpenXR for the desired targets in Project Settings.
3. Run `Setup/Create or Update VR Player Prefab`.
4. Run `Setup/Prepare Current Scene Interactables for VR` in each scene.
5. Run the OpenXR/XRI Project Validation checks.

The VR prefab is generated by the installed version of XRI and includes adapters for hands, haptics and 3D UI Toolkit. Controller/hand models are replaced under their `ModelSocket`.

## 8. Accessibility and horror pacing

From the game's own settings menu (not the Editor's), the player can enable:

- reduced flashes, camera shake and loud sounds;
- chase assistance (the enemy is somewhat slower and forgets sooner);
- gore reduction, available as an option even though the base template does not include any gore content yet.

None of these options replaces the difficulty setting: they are independent, so a player can combine `Nightmare` with `chaseAssistance` if needed.

If you add a `TensionDirector` to the scene, it limits how many horror events can fire in a row (global cooldown and budget per time window), on top of each event's own cooldown. It is optional: without it, everything works as before.

## 9. Publishing

Before distributing the asset:

1. Run `Validation/Run Framework Smoke Tests`.
2. Run `Validation/Validate Save IDs` in each scene.
3. Test PC and VR separately.
4. Do not change `SaveId` or `ItemId` in a published update without adding a migration.
5. Run `Validation/Validate Current Scene` and solve every puzzle in each scene from a clean build.
