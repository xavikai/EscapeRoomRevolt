# Changelog

All notable changes to Escape Room Revolt are documented here.

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
