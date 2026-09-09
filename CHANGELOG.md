# Changelog

All notable changes to Escape Room Revolt are documented here.

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
