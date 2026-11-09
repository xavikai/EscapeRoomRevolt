# Changelog

All notable changes to Escape Room Revolt are documented here.

## [0.2.1] - 2026-11-09

Documentation and menu-theming update for the first class sessions. Release notes: `RELEASE_NOTES_v0.2.1.md`.

### Added

- `MenuThemeSettings` gains an **Imágenes** section: `Screen Background` (full-screen image behind the main menu, optional in the pause menu via `Screen Background In Pause`), `Panel Image` with `Panel Image Slice` (9-slice frame) and `Button Image` / `Button Hover Image` with `Button Image Slice`. Students can change the menu background, frame and buttons from the Inspector without UI Toolkit. Images are skipped in high-contrast mode; theme images override USS backgrounds, as theme colours already did.
- Interactive guide: the start section now begins inside Unity with the template open (no Unity installation or editor navigation), embeds Unity screenshots with a zoom viewer, and adds a phase on non-puzzle pieces: Narrative Trigger voice/subtitle zones, Hint Zone with Hint Data, Event Trigger Zone actions, Game Over Timer, Moving Hazard, Ending Definition + Game End Trigger, objectives (Objective Definition/Set/Manager), Generic Trigger, Examine Hotspot and Room Portal. CA/ES/EN.
- Interactive guide, menu design: three new level-1 steps for the theme images, with screenshots of the theme asset and the Sprite import settings.

### Fixed

- Portable 0.2.1 resource correction: restored the seven original XRI hand icons and a shared cycle icon from the installed official XRI sample, replaced missing simulator/dropdown sprites with available sample/built-in resources, removed obsolete sample baked-lighting and optional TMP EmojiOne references. Portable validation now audits serialized GUIDs, asset object references and shader errors, and resolves the Windows output version from PlayerSettings.
- Unity 6.6 compatibility: replaced compile-error InstanceID APIs with EntityId, removed obsolete FindObjectsSortMode/FindFirstObjectByType calls, and use Unity's reload-aware loaded assembly list in 6.6. This restores compilation of the Editor assembly that supplies the Escape Room Framework menu; Unity 6.4 uses the compatible assembly-query fallback.
- Escape Room flashlight battery is unlimited in Auto mode; Survival and Custom Hybrid retain draining, and `Drains` explicitly enables battery puzzles. The battery HUD is hidden when consumption is disabled. Dropped equippable items now fall with gravity by default (`Fall When Dropped` can restore a stationary drop).
- Product Windows validation resolves the build folder from `PlayerSettings.bundleVersion` instead of the historical beta folder.
- The main-menu PlayMode test checks the translated labels in the active language instead of assuming Spanish when settings are already saved.
- World and panel text (keypad screens, number wheels, room titles) rendered as broken/pink: the Escape Room consumer project only shipped `TMP_SDF-Mobile.shader` from the TextMesh Pro essential resources, without `TMPro_Properties.cginc` and the other includes, so the font material's shader failed to compile (`Couldn't open include file 'TMPro_Properties.cginc'`). Restored the full `Assets/TextMesh Pro/Shaders` folder with its original GUIDs. The Survival project has the same gap and still needs the same files.
- Museum room 11 (chained puzzles): the levers tilted on X, towards the player, so a step was barely visible; they now tilt sideways (Z) in the museum scenes and in the `State Puzzle` / `Multi-Stage Puzzle` creators. The room sign said «any order» while the levers stay locked until the colour sequence is solved; it now reads «Colours first · then the levers» (CA/ES/EN).
- The `Interactable Note` custom Inspector threw a NullReferenceException that hid `Note Content`; it now draws every field null-safely and relies on the component headers.
- Release builds are now versioned 0.2.1 (`PlayerSettings.bundleVersion`) and written to `Builds/Release/v0.2.1/`, replacing the historical `v0.1.0-beta.5` folder name.
- The Game Over Timer wizard's default HUD label is now the catalogue key `TIEMPO RESTANTE`, so it is translated in CA/ES/EN.

## [0.2.0] - 2026-11-07

First Escape Room release as an independent product project (`EscapeRoom/Unity`) on shared packages. Ships the validated **0.2.0-pcfix-2026-10-26** Windows demo and portable source delivery; no code changes since that validation. Release notes: `RELEASE_NOTES_v0.2.0.md`. Manual visual QA and VR hardware QA remain pending.

### Repository and delivery - 2026-10-26


- Updated the shared interactive guide with separate product paths, package ownership and current validation limits in CA/ES/EN while preserving original Three.js stage implementations. Added a dedicated Catalan Survival guide with educational detection, camera/evidence and checkpoint simulations, controls, authoring steps and QA links. Static/Node validation only; no application control or visual QA.

- Organised the repository into EscapeRoom and Survival product folders with Unity projects, documentation, portable exports, source deliveries and Windows demos. Kept canonical shared packages, tools and historical authoring under EscapeRoom/Framework; updated consumer UPM paths, tooling layout resolution and live documentation links. Preserved historical reports and delivery contents; no application control or Unity rerun.

- Added ready-to-run Windows demo ZIPs with complete runtime dependencies, manual QA guide, product-specific pending records and prior build evidence. Verified EXE/boot/Data hashes against successful builds, copied runtime files unchanged, omitted Burst DoNotShip diagnostics and verified every ZIP entry. No Unity rerun or executable launch; visual acceptance remains pending.

- Prepared separate PC delivery ZIPs containing unchanged validated portable projects, the manual QA guide, product-specific pending records and prior validation reports. Verified every ZIP file by SHA-256, exact file lists and source snapshot hashes; included per-file manifests and archive checksum sidecars. No Unity rerun or application control; manual QA remains pending.

- Added a manual PC visual QA guide covering shared flows, the current 14-room Escape Room museum and the Survival objective chain, with expected results, evidence requirements and an unexecuted session/issue template. Linked product and readiness documentation; manual acceptance remains pending. No application UI control resumed.

- Refreshed portable snapshots as 0.2.0-pcfix-2026-10-26 with the PC corrections. Both pass cold imports, package/assembly isolation audits, full test suites and Windows builds in temporary projects outside the repository; native XR pre-init is absent. Original snapshots preserved. No UI control resumed.

#### PC QA corrections

- Shipped implicit TextMesh Pro Resources settings and their dependencies in both consumers; added generation/validation coverage and exercised world-text mesh updates during product startup tests.
- Desktop builds temporarily remove XR loaders as well as disabling automatic initialization. Authoring XR settings are restored even on build failure; a new Editor regression covers the scope and Windows verification rejects native XR pre-init in boot.config.
- Fixed the Survival navigation helper namespace, baked persistent navigation data into the consumer scene and verified enemy spawn sampling and active agents on the loaded mesh. Future demo construction also persists the bake.
- Confirmed batch validation and Windows rebuilds: Escape Room 26/26 EditMode + 34/34 PlayMode; Survival 26/26 + 38/38. No skipped tests. Application control and visual QA were not resumed.
- Preserved earlier portable snapshots as historical artifacts; their portability results do not include these later PC corrections. See docs/QA_PC_2026-10-26.md and its JSON evidence.

### Separation work - 2026-10-25

#### Independent product projects

- Extracted canonical source into Common and Survival local UPM packages, version 0.2.0, including shared Editor and tests.
- Created Projects/EscapeRoom and Projects/Survival with separate Assets, configuration, item catalogs, application identity, data directories and package manifests/locks. Consumers exclude MCP; Escape Room excludes Survival and AI Navigation.
- Separated Survival HUD templates/styles from Common; preserved the binary Survival demo byte for byte with dependencies exported by AssetDatabase.
- Added safe generation, repeatable validation and relocatable export tools; all refuse to overwrite existing projects or deliveries.
- Consumer tests pass without skips: Escape Room 25 EditMode / 34 PlayMode; Survival 25 EditMode / 38 PlayMode, including real first-scene startup.
- Windows builds succeeded for both consumers. Serialized reference checks passed for Escape Room's seven scenes/five prefabs and Survival's four scenes/ten prefabs. Portable snapshots also passed clean Unity imports in relocated temporary projects outside the source repository, embedded package/compiled assembly path audits and full EditMode/PlayMode suites. Original export tree hashes were unchanged.

#### Changed

- Began preparing separate Escape Room and Survival products on a shared base. Survival runtime, UI, authoring tools and tests now have their own assemblies; shared runtime assemblies do not reference Survival.
- Added optional composition contracts (`GameModules`, `IGameDifficultyProvider`, `IPickupRemovalHandler`). Survival owns service/player/HUD installation, puzzle sanity penalties and pickup checkpoint integration.
- Moved reusable flashlight, camera shake, visual replacement and noise components into `Systems/Options`, preserving public namespaces and Unity script GUIDs. Flashlight consumption uses a neutral multiplier without a difficulty provider.
- Extracted sanity presentation from the shared gameplay UI; the menu uses a product-neutral difficulty contract. Survival difficulty releases its provider and manual-save gate when destroyed.
- Moved Survival authoring into its Editor assembly. The hiding-spot menu path is unchanged; its API owner is now `SurvivalInteractableCreator`.
- Updated the architecture documentation and separation backlog. Standalone product projects, package extraction, asset pruning and optional XR installation are still pending.

#### Added

- Three EditMode tests for optional module registration, stress dispatch and neutral flashlight consumption; four Survival PlayMode regressions for feature gating and checkpoint integration.
- Product module validator for compiled assembly boundaries and serialized scene/prefab references; exported results in `docs/validation/separation-2026-10-25.json`.

#### First-phase validation before package extraction

- Unity `6000.4.9f1` headless batch compilation succeeded; **25/25 EditMode and 37/37 PlayMode** tests passed, with none skipped.
- Five shared assemblies have no Survival assembly references; eight enabled scenes and thirteen template prefabs have no detected missing scripts or broken serialized references. All ten moved script GUIDs were preserved.
- No product builds, clean distribution import or physical VR acceptance were performed in this phase.

## [0.1.0-beta.5] - 2026-09-30

### Changed

- Synchronized the release source with the default branch and tracked Unity metadata for the new documentation and localization script.
- Updated the Windows and Quest release builder version identifiers to `v0.1.0-beta.5`. No new binaries are attached to this source release.

## [0.1.0-beta.4] - 2026-09-29 review

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
