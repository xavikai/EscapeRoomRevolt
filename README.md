# Escape Room Revolt

A Unity 6 (URP) framework for building first-person **Escape Room** and **Survival Horror** games from the same codebase, for PC and VR (XR Interaction Toolkit).

## Vision

Not a single closed room — a reusable framework where rooms, puzzles, enemies and objectives are configured through `ScriptableObjects` and Editor tools, not by rewriting code. A `GenreFeatureSettings` profile (`EscapeRoom`, `SurvivalHorror` or `CustomHybrid`) turns optional systems (flashlight, sanity, hostile AI, hiding spots, night vision) on or off per project without branching shared code.

## Requirements

- Unity `6000.4.9f1`
- Universal Render Pipeline `17.4.0`
- Input System `1.20.0`
- XR Interaction Toolkit `3.3.0` (only needed for VR; PC works without it)

## Architecture principles

1. **Data, logic and presentation are separate.** Configuration lives in `ScriptableObjects`; gameplay logic lives in small reusable components; visuals are swappable children under a `ModelSocket` without touching scripts or colliders.
2. **Small shared interfaces.** `IInteractable` for interaction, `ISaveable` for persistence — puzzles, inventory, saving and endings don't know or care whether the input came from a mouse or an XR controller.
3. **One player-input source.** `InputRouter` and `PlayerPlatformRegistry.Current` are the only things gameplay code queries — never `Camera.main` or a vendor VR API directly.
4. **State-based saving.** Every persistent object registers with `SaveId` and serializes itself; `SaveManager` writes slots atomically (`File.Replace`) with automatic `.bak` recovery.

## What's in the template

- **Interaction & authoring** — raycast (PC) and XRI (VR) share one `InteractionDispatcher`; doors, drawers, cabinets, levers, switches, notes and pickables are all created from the `Escape Room Framework > Create` Editor menu with example data pre-filled.
- **Inventory** — quantities, hotbar, guided combination, 3D examination with clickable `ExamineHotspot`s, equipment via `ModelSocket`.
- **Eight puzzle types** — code panel, sequence, state, socket, multi-stage (branching), sliding (15-puzzle), wire (connect-the-cables) and pipe (rotate-to-connect). All share reset, progressive hints and optional seeded randomized variants.
- **Save/Load** — three manual slots, quick save/load, thumbnails, atomic writes with backup recovery, versioned per-object state.
- **Survival Horror** — modular flashlight and night-vision camcorder, sanity with visual/audio/haptic feedback, patrol/perception/chase AI with two enemy archetypes, hideable lockers/beds/containers with AI inspection, checkpoints, typed damage, PC/VR-shared traversal (vault/climb/ladder/squeeze), evasion (lean/look-back/slide), a tension director that rate-limits horror events, and data-driven difficulty presets.
- **UI Toolkit menus** — main menu, pause, settings, save/load, credits, results, all Canvas-free. Re-skinnable from a single `MenuThemeSettings` asset (colors, fonts, logo) without touching code or USS, plus a built-in high-contrast accessibility mode.
- **VR** — a generated `Player_VR` prefab (XRI Starter Assets), per-hand equipment and haptics, world-space UI Toolkit via `VRUIToolkitPresenter`, and an XRI tunneling vignette wired to continuous move/turn.

## Demo scenes

| Scene | Genre | What it shows |
|---|---|---|
| `MainMenu` | — | Entry point, profile-agnostic. |
| `LockedOffice` | Escape Room | Small vertical slice: key, drawer, note, safe code, socket puzzle. |
| `ShowcaseMuseum` | Escape Room | Larger sandbox covering most interactables and all eight puzzle types. |
| `SurvivalHorrorDemo` | Survival Horror | A verified vertical slice: a four-objective chain (recover batteries → restore power → record evidence → escape), an enemy, two hiding spots, noise-distraction throwables, four traversal challenges, a double checkpoint and a victory ending. |
| `VRTemplate` | — | Minimal XRI rig, teleport, grab and a simple interactable. |

## Getting started

The supported entry point is the **Escape Room Framework** Editor menu — legacy scene generators and destructive commands are intentionally hidden from it.

1. `Escape Room Framework > Validation > Run Framework Smoke Tests`
2. `Escape Room Framework > Setup > Create or Update Main Menu Scene` (once)
3. For VR: let Package Manager import OpenXR / XR Plug-in Management / XRI, then `Escape Room Framework > Setup > Create or Update VR Player Prefab`, then `Setup > Prepare Current Scene Interactables for VR` per scene.
4. Build a room from `Escape Room Framework > Create > ...` (interactables, puzzles, survival systems), assigning a `ScriptableObject` per item/puzzle/hint as needed.

## Documentation

- [`Assets/_EscapeRoomTemplate/ROADMAP.md`](Assets/_EscapeRoomTemplate/ROADMAP.md) — the living status document: what's verified, what's pending, in what order. Read this first for "where are we now."
- [`Assets/_EscapeRoomTemplate/PROGRAMMING_GUIDE.md`](Assets/_EscapeRoomTemplate/PROGRAMMING_GUIDE.md) — API-level guide to the systems above, with code snippets.
- [`Assets/_EscapeRoomTemplate/DOCUMENTACIO_COMPLETA.md`](Assets/_EscapeRoomTemplate/DOCUMENTACIO_COMPLETA.md) — exhaustive architecture and authoring reference.
- [`Assets/_EscapeRoomTemplate/UserManual.md`](Assets/_EscapeRoomTemplate/UserManual.md) — end-player-facing controls and menu reference.

## Known gaps before commercial release

Tracked in detail in `ROADMAP.md`. In short: no automated test suite yet (blocked on splitting the runtime into `asmdef` assemblies first), no `ThirdPartyNotices` / asset license inventory yet, and UI/gameplay text is not yet localized — it ships in Spanish only. VR is functionally complete but not yet QA'd on real hardware.
