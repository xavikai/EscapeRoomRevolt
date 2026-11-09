# Escape Room Revolt

**Current release: [Escape Room 0.2.1](https://github.com/xavikai/EscapeRoomRevolt/releases/tag/escaperoom-v0.2.1)** — [release notes](RELEASE_NOTES_v0.2.1.md), [downloadable interactive guide](https://github.com/xavikai/EscapeRoomRevolt/releases/download/escaperoom-v0.2.1/guia-escape-room-0.2.1.html) and [9 November validation](docs/validation/release-0.2.1.json). Escape Room passes 26 EditMode and 34 PlayMode tests and its Windows build. Survival's last complete validation is dated 26 October; it has no published release assets. Manual PC and VR hardware acceptance remain pending.

**Carpetes reorganitzades el 26/10/2026:** consulta [l’índex actual del repositori](../../README.md). Les rutes antigues en informes JSON i exemples històrics identifiquen l’estat de la validació; el mapa de trasllat en documenta la ubicació actual.

**PC QA corrections (26 October 2026):** [confirmed fixes, tests and Windows rebuilds](docs/QA_PC_2026-10-26.md). TMP defaults are shipped, native desktop XR pre-init is disabled and Survival navigation is persisted. Validation remained headless; final visual reinspection is pending.

**Independent Unity projects:** open [`EscapeRoom/Unity`](../Unity) or [`Survival/Unity`](../../Survival/Unity) in Unity 6000.4.9f1. Both consume the canonical source in `EscapeRoom/Framework/SharedPackages`; Survival is absent from the Escape Room installation. See [configuration, packages, validation and portable exports](docs/PROJECTES_INDEPENDENTS.md). `EscapeRoom/Framework` remains the historical authoring project. The 26 October portable snapshots passed clean imports outside this repository; those results do not certify a clean import of the newer 0.2.1 ZIP.

**Interactive guide (CA/ES/EN):** open [`docs/guia/index.html`](docs/guia/index.html) in a browser for a hands-on tour of the mechanics, menus and PC/VR players.

**Escape Room release preparation:** start with the buyer quick start ([castellano](Assets/_EscapeRoomTemplate/Documentation/ESCAPE_ROOM_QUICKSTART.md) · [català](Assets/_EscapeRoomTemplate/Documentation/ESCAPE_ROOM_QUICKSTART.ca.md) · [English](Assets/_EscapeRoomTemplate/Documentation/ESCAPE_ROOM_QUICKSTART.en.md)), then read the [September 2026 audit](Assets/_EscapeRoomTemplate/AUDITORIA_ESCAPE_ROOM_2026-09-09.md) and [release gates](Assets/_EscapeRoomTemplate/COMMERCIAL_READINESS.md). Survival Horror is outside this edition's commercial verification. VR hardware acceptance remains pending.

A Unity 6 (URP) framework for building first-person **Escape Room** and **Survival Horror** games on a shared base, for PC and VR (XR Interaction Toolkit). The two product projects have their own assets, configuration and package manifests; framework code is maintained once in local UPM packages. Survival's PC demo is available; its Quest demo remains unvalidated.

## Vision

Rooms, puzzles, enemies and objectives are configured through `ScriptableObjects` and Editor tools. A `GenreFeatureSettings` profile (`EscapeRoom`, `SurvivalHorror` or `CustomHybrid`) selects optional systems. `GameModules` lets Survival install its services and HUD without concrete references from the shared bootstrap, puzzles, pickups or menu. Reusable options such as the flashlight remain in the shared Systems assembly. Profiles select behavior; assemblies establish code ownership.

## Requirements

- Unity `6000.4.9f1`
- Universal Render Pipeline `17.4.0`
- Input System `1.20.0`
- XR Interaction Toolkit `3.3.0`, XR Management `4.5.3`, OpenXR `1.16.1` (keep these installed for PC too: the shared Player assembly references XR types)

## Architecture principles

1. **Data, logic and presentation are separate.** Configuration lives in `ScriptableObjects`; gameplay logic lives in small reusable components; visuals are swappable children under a `ModelSocket` without touching scripts or colliders.
2. **Small shared interfaces.** `IInteractable` for interaction, `ISaveable` for persistence — puzzles, inventory, saving and endings don't know or care whether the input came from a mouse or an XR controller.
3. **One player-input source.** `InputRouter` and `PlayerPlatformRegistry.Current` are the only things gameplay code queries — never `Camera.main` or a vendor VR API directly.
4. **State-based saving.** Every persistent object registers with `SaveId` and serializes itself; `SaveManager` writes slots atomically (`File.Replace`) with automatic `.bak` recovery.

## What's in the template

- **Interaction & authoring** — raycast (PC) and XRI (VR) share one `InteractionDispatcher`; doors, drawers, cabinets, levers, switches, notes and pickables are all created from the `Escape Room Framework > Create` Editor menu with example data pre-filled.
- **Inventory** — quantities, hotbar, guided combination, 3D examination with clickable `ExamineHotspot`s, equipment via `ModelSocket`.
- **Ten puzzle controllers** — code panel, sequence, state, socket, throw, placement, chained puzzle groups, sliding, pipe (rotate-to-connect) and linked lights (a Lights Out circuit). A chained group keeps every child puzzle visible and can require free or ordered completion before firing one shared payoff. Melody is a presentation of the shared sequence solver. All share reset, persistence and progressive-hint integration; selected types support seeded variants.
- **Independent fail-state mechanics** — a `MovingHazard` that travels between arbitrary 3D markers (wall, ceiling, floor, platform or water) and a separate optional `GameOverTimer` presented in the shared gameplay HUD.
- **Save/Load** — three manual slots, quick save/load, thumbnails, atomic writes with backup recovery, versioned per-object state.
- **Survival Horror** — modular flashlight and night-vision camcorder, sanity with visual/audio/haptic feedback, patrol/perception/chase AI with two enemy archetypes, hideable lockers/beds/containers with AI inspection, checkpoints, typed damage, PC/VR-shared traversal (vault/climb/ladder/squeeze), evasion (lean/look-back/slide), a tension director that rate-limits horror events, and data-driven difficulty presets.
- **UI Toolkit menus** — main menu, pause, settings, save/load, credits, results, all Canvas-free. Re-skinnable from `MenuThemeSettings` (colors, fonts, logo, screen background, panel frame and normal/hover button images with slicing). USS remains available for custom styles. High-contrast mode omits theme images for readability.
- **VR** — a generated `Player_VR` prefab (XRI Starter Assets), per-hand equipment and haptics, world-space UI Toolkit via `VRUIToolkitPresenter`, and an XRI tunneling vignette wired to continuous move/turn.

## Demo scenes

The PC museum now includes **Room 14: Linked Lights**, a configurable circuit with saved progress, reset and a reusable prefab. See the [room and hint-zone authoring guide](Assets/_EscapeRoomTemplate/Documentation/LINKED_LIGHTS_AND_HINT_ZONES.md).

| Scene | Genre | What it shows |
|---|---|---|
| `Intro` | — | Optional logo/cutscene sequence before the menu. |
| `MainMenu` | — | Entry point, profile-agnostic. |
| `LockedOffice` | Escape Room | Small vertical slice: keys, doors, drawer, note, keypad/safe flow and persistence. |
| `ShowcaseMuseum` | Escape Room | PC museum covering all puzzle controllers, visible chained puzzle groups, clickable number-wheel arrows, an arbitrary-direction moving hazard and an independent HUD Game Over timer. |
| `ShowcaseMuseumVR` | Escape Room VR | VR counterpart of the museum, with the same Room 11 and Room 13 gameplay and an XR player rig. |
| `SurvivalHorrorDemo` | Survival Horror | A verified vertical slice: a four-objective chain (recover batteries → restore power → record evidence → escape), an enemy, two hiding spots, noise-distraction throwables, four traversal challenges, a double checkpoint and a victory ending. |
| `VRTemplate` | — | Minimal XRI rig, teleport, grab and a simple interactable. |
| `LockedOfficeVR` | Escape Room VR | VR counterpart of the compact Locked Office example. |

## Downloads

Published versions are available from [GitHub Releases](https://github.com/xavikai/EscapeRoomRevolt/releases). Release assets identify the target explicitly: Windows PC or Meta Quest/OpenXR Android. VR releases remain beta until the hardware QA matrix in `ROADMAP.md` is complete.

## Getting started

The supported entry point is the **Escape Room Framework** Editor menu — legacy scene generators and destructive commands are intentionally hidden from it.

1. `Escape Room Framework > Validation > Run Framework Smoke Tests`
2. `Escape Room Framework > Setup > Create or Update Main Menu Scene` (once)
3. For VR: let Package Manager import OpenXR / XR Plug-in Management / XRI, then `Escape Room Framework > Setup > Create or Update VR Player Prefab`, then `Setup > Prepare Current Scene Interactables for VR` per scene.
4. Build a room from `Escape Room Framework > Create > ...` (interactables, puzzles, survival systems), assigning a `ScriptableObject` per item/puzzle/hint as needed.

## Documentation

- [`Assets/_EscapeRoomTemplate/ROADMAP.md`](Assets/_EscapeRoomTemplate/ROADMAP.md) — the living status document: what's verified, what's pending, in what order. Read this first for "where are we now."
- [`Assets/_EscapeRoomTemplate/AUDITORIA_ESCAPE_ROOM_2026-09-09.md`](Assets/_EscapeRoomTemplate/AUDITORIA_ESCAPE_ROOM_2026-09-09.md) — historical September audit (tests, builds, scene inspection). The [August audit](Assets/_EscapeRoomTemplate/AUDITORIA_ESCAPE_ROOM_2026-08-09.md) keeps the historical room-by-room walkthrough. For 0.2.1, use the release notes and validation linked above.
- [`docs/guia/index.html`](docs/guia/index.html) — interactive guide for people using the template (mechanics playground, menus, PC/VR player, feature tour).
- [`Assets/_EscapeRoomTemplate/PROGRAMMING_GUIDE.md`](Assets/_EscapeRoomTemplate/PROGRAMMING_GUIDE.md) — API-level guide to the systems above, with code snippets.
- [`Assets/_EscapeRoomTemplate/DOCUMENTACIO_COMPLETA.md`](Assets/_EscapeRoomTemplate/DOCUMENTACIO_COMPLETA.md) — exhaustive architecture and authoring reference.
- [`Assets/_EscapeRoomTemplate/UserManual.md`](Assets/_EscapeRoomTemplate/UserManual.md) — end-player-facing controls and menu reference ([català](Assets/_EscapeRoomTemplate/UserManual.ca.md) · [English](Assets/_EscapeRoomTemplate/UserManual.en.md)).
- Step-by-step recipes for every mechanic: [català](Assets/_EscapeRoomTemplate/Documentation/RECEPTES.md) · [castellano](Assets/_EscapeRoomTemplate/Documentation/RECETAS.md) · [English](Assets/_EscapeRoomTemplate/Documentation/RECIPES.md).
- [`CHANGELOG.md`](CHANGELOG.md) — version history and migration-impact summary.

## Known gaps before commercial release

Tracked in detail in `ROADMAP.md` and [the product guide](docs/PROJECTES_INDEPENDENTS.md). The independent consumers pass headless batch tests in Unity 6000.4.9f1: **Escape Room 26/26 EditMode + 34/34 PlayMode; Survival 26/26 EditMode + 38/38 PlayMode**. Startup of the real product scenes and their HUD composition are covered. `ShowcaseMuseumVR` does not yet include Room 14 (Linked Lights). The Escape Room puzzle definitions and Pipe payoff are closed. Audio licenses remain to be confirmed in `ThirdPartyNotices.md`, localization covers ES/EN/CA, a final human build playthrough is pending, and VR has not yet passed QA on real hardware.

**Portable PC correction snapshots (26 October 2026):** `Distributions/EscapeRoom-0.2.0-pcfix-2026-10-26` and `Distributions/Survival-0.2.0-pcfix-2026-10-26` include the TMP, desktop XR and Survival navigation corrections. Both pass fresh imports, isolation audits, complete automated suites and Windows builds outside this repository. See [the portable validation report](docs/validation/portable-exports-2026-10-26.json). No application UI control or visual QA was resumed.

Manual PC QA is ready to run: [steps and expected results](docs/QA_VISUAL_MANUAL_PC.md), [session and issue template](docs/QA_VISUAL_MANUAL_PC_REGISTRE.md). All manual cases remain pending; preparing this guide did not resume application control.

**PC delivery bundles:** [ZIP packages, contents and handoff instructions](docs/LLIURAMENT_PC_2026-10-26.md). Each contains the unchanged validated portable project, manual QA guide, product-specific pending record and validation evidence. Archive contents and SHA-256 verified; manual QA remains pending.

**Ready-to-run Windows demos:** [downloadable local ZIPs and instructions](docs/DEMOS_WINDOWS_2026-10-26.md) include the complete runtime, manual QA guide and product-specific pending record. No Unity installation is needed to play. Build provenance and archive contents verified; executables were not launched and visual QA remains pending.
