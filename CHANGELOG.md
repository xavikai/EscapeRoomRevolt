# Changelog

All notable changes to Escape Room Revolt are documented here.

## [Unreleased] - 2026-09-29 review

### Fixed

- Loading a game from the main menu (**Continue** / **Load**) or from the pause menu no longer leaves the player frozen. The menu and HUD now release `GameplayBlockState` when they are unloaded with a screen or panel open.
- `StatePuzzle` now saves and restores the position of every lever/wheel it watches. Half-dialled combinations survive a load, and a solved lock reappears at its answer instead of at the starting index (older saves included).
- `SteppedPositioner` keeps an index restored before `Start` (save/load, `PipeTileButton` sync) instead of snapping back to its starting index; new `IndexSynced` event so `NumberWheelView` digits refresh after a load.
- `MultiStagePuzzle` re-synchronises with its children after the whole save is restored, so ordered groups no longer lock the wrong child after loading.
- `PipeTileButton` no longer turns the tile art once the puzzle is solved (visual and logic stayed out of sync).
- `PlacementPuzzle` only restores placements for a solved board. Loose pieces respawn at their start, so partial placements left "ghost" pieces in sockets.
- `CodePanelPuzzle` restores a randomised code before re-applying a solved display.
- `InteractableToggle` now saves its on/off state (optional `Invoke Event On Load`) and no longer drifts when a `Custom Pivot` is assigned.
- `InteractableTrigger` saves its used/toggle state. A spent single-use trigger stays in the scene instead of being deleted on load.
- `InteractionManager` and `PhysicsGrabber` remove only a duplicate component, never the player's camera object.
- `HintManager` clears the previous scene's puzzle context when a new scene loads.
- `ThrowTarget` repaints hit/solved colours after a load.

### Added

- `MenuThemeSettings.mainMenuTitle` and `MenuThemeSettings.creditsText` so a new game can set its own title and credits without code.
- Every menu label, button, slider, toggle and confirmation now goes through `LocalizationService.Tr`, so adding a catalog entry is enough to translate it.
- Six PlayMode regressions in `SaveRestoreRegressionTests`.
- Interactive template guide (CA/ES/EN, three.js) in `docs/guia/index.html`, with a step-by-step recipe for every mechanic and room system (also as Markdown: `Documentation/RECEPTES.md`, `RECETAS.md`, `RECIPES.md`).
- Full localization: `DefaultLocalizationCatalog` now covers menus, HUD, inventory, notes, keypad, VR panel, subtitles, hints, objectives, item texts, default prompts and save errors in Spanish, English and **Catalan** (~500 entries). New `SceneTextLocalizer` translates 3D signs (TextMeshPro/TextMesh) whose text is a catalog key. `LocalizationCatalog` gains `HasKey` and a dictionary lookup.
- Catalan and English versions of `UserManual` and `ESCAPE_ROOM_QUICKSTART`.
- Hints for the pipe, placement and sliding demo puzzles (their hint assets were empty).

### Changed

- Scene and data texts are unified in Spanish (the catalog key language): mixed Catalan/English prompts, signs, item names, hints and objectives in `ShowcaseMuseum`, `ShowcaseMuseumVR`, `LockedOffice` and the demo assets were rewritten. Prompts such as `[E] Open Cabinet` no longer produce a doubled `[E]` in the HUD.
- Default prompts of interactables and puzzle helpers are Spanish catalog keys (`Interactuar`, `Abrir`, `Cerrar`...).

## [0.1.0-beta.3] - 2026-09-11

### Added

- Room 14 (Linked Lights): a configurable circuit puzzle with a reusable prefab, progressive hints, saved partial progress, reset button and door reward.
- Five PlayMode regression tests covering circuit light toggling, solver reset, hint zone transitions and trigger re-entry (`HintAndCircuitTests`).
- Authoring guide and documentation in `Assets/_EscapeRoomTemplate/Documentation/LINKED_LIGHTS_AND_HINT_ZONES.md`.

### Changed & Improved

- Hardened hint and narrative zones (`HintZoneTrigger`, `NarrativeTrigger`) with kinematic trigger bodies and player-parent tag detection.
- Exiting an old hint zone no longer clears a newly entered zone's context.
- Museum audio trigger is now replayable after player exit/re-entry and cooldown, while preserving `Once` mode for authored narrative sequences.


## [0.1.0-beta.2] - 2026-09-09

### Added

- Comprehensive buyer quick-start guide: `Assets/_EscapeRoomTemplate/Documentation/ESCAPE_ROOM_QUICKSTART.md` accessible via editor menu `Escape Room Framework > Documentation > Open Escape Room Quick Start`.
- September 2026 commercial audit and verification results (`AUDITORIA_ESCAPE_ROOM_2026-09-09.md` and `AUDIT_RESULTS_2026-09-09.json`).
- `SaveRecoveryTests` suite covering corrupted saves, `.bak` file recovery, version validation and seed roundtrips (20 EditMode tests total).
- `CommercialRegressionTests` suite covering VR hardware interaction grab collisions, socket ownership, and dual-hand locks (23 PlayMode tests total).
- `SceneFeatureOverride` component to support scene-level feature toggling.

### Fixed

- Prevented physical sockets and socket receivers from snatching pieces currently held by `VRHardwareInteractor`.
- Prevented dual-hand grabbing of the same physical object simultaneously in VR.
- Blocked VR hardware interaction raycast when menus or pause are active, clearing focus and preventing interaction bleed-through.
- Ensured `InteractableBase.CanInteract` checks `isActiveAndEnabled` so disabled components cannot be triggered.
- Fixed `SaveManager` fallback recovery when the primary `.json` is missing or corrupted, and validated version/data integrity before restoration.
- Fixed entity save state alignment so serialisation errors do not desynchronise keys and values.
- Restored previous game flow state when scene loading fails or is aborted.
- Handled empty sequence edge-case and restored input prefix in `SequencePuzzle`.
- Prevented `StatePuzzle` from considering incomplete conditions as satisfied.
- Reconstructed placed piece visuals idempotently upon game loading in `SocketPuzzle`.
- Fixed missing GUID action references in the XRI simulator prefab.
- Isolated desktop release builds so Windows standalone does not initialize VR runtime on startup.

### Changed

- Re-aligned `COMMERCIAL_READINESS.md` and `UserManual.md` with current verification scope.
- Configured OpenXR on Android with `PrioritizeInputPolling`.
- Updated release pipeline to package `v0.1.0-beta.2` standalone Windows and Meta Quest builds.

## [0.1.0-beta.1] - 2026-08-13

### Added

- `ShowcaseMuseumVR` and `LockedOfficeVR` demonstration scenes.
- Shared VR gameplay panel, hardware interaction bridge and opaque-camera guard.
- Physical ▲/▼ controls above and below every number wheel.
- `NumberWheelStepButton`, using the same `TryStep(+1/-1)` path as the VR controls.
- Expandable chained-puzzle entries for coordinating any number of visible child puzzles.

### Changed

- Room 11 now keeps the sequence and lever puzzles visible simultaneously. The final door opens only after every child is solved.
- Chained puzzles support free completion or ordered unlocking without hiding future puzzle models.
- Room 13 uses mouse-clickable physical arrows on PC; W/S and keyboard arrow control were removed.
- `InteractionManager` supports pointer-position raycasts and left-click interaction while a puzzle owns the unlocked cursor.
- PC and VR museum scenes now share the Room 11 and Room 13 behavior.
- Documentation now distinguishes `ShowcaseMuseumVR` from the minimal `VRTemplate` scene.

### Validation

- Unity scripts compile without errors on Unity `6000.4.9f1`.
- Ordered and free-order chained-puzzle modes pass the functional validation.
- Both museum scenes contain the two Room 11 puzzle groups and eight Room 13 arrow buttons.

### Known limitations

- This is a beta release. Full headset QA, performance profiling and the device matrix tracked as `VR-007` remain pending.
- Third-party audio redistribution must be confirmed before a commercial asset-store release.

[0.1.0-beta.1]: https://github.com/xavikai/EscapeRoomRevolt/releases/tag/v0.1.0-beta.1
