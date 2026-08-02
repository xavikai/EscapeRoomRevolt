# Escape Room Framework - Programming Guide

For the exhaustive architecture, authoring tutorials, API examples, VR workflow and troubleshooting reference, see [DOCUMENTACIO_COMPLETA.md](DOCUMENTACIO_COMPLETA.md).

## Safe setup workflow

The supported entry point is the `Escape Room Framework` menu. Setup commands are idempotent or request confirmation before replacing generated assets. Legacy scene generators, automatic package installation and direct destructive cleanup commands are intentionally hidden.

1. Run `Validation/Run Framework Smoke Tests`.
2. Run `Setup/Create or Update Main Menu Scene` once. It creates `MainMenu.unity`, `GameFlowSettings` and places the menu first in Build Settings.
3. For VR, let Package Manager import OpenXR, XR Plug-in Management and XRI, then configure the desired OpenXR profiles in Project Settings.
4. Run `Setup/Create or Update VR Player Prefab`. It delegates the rig hierarchy to Unity's installed XRI version and saves `Player_VR.prefab`.
5. In each VR scene, run `Setup/Prepare Current Scene Interactables for VR`. Existing gameplay components are preserved.

## Genre profiles

`GenreFeatureSettings` is the project-wide source of truth. The asset lives at `Resources/GenreFeatureSettings.asset`; designers choose it from `Escape Room Framework/Configuration`.

```csharp
using EscapeRoomRevolt.Core.Settings;

if (GameFeatures.IsEnabled(OptionalGameFeature.Flashlight))
{
    // Run or present flashlight-specific behaviour.
}
```

`EscapeRoom` enables no optional horror feature. `SurvivalHorror` enables `Flashlight`, `Sanity` and `HorrorEvents`. `CustomHybrid` uses the serialized bit mask. Shared systems must not branch on genre; only optional systems query feature flags. This keeps puzzles, inventory, saves, endings, PC and VR reusable in every profile.

Runtime components disable themselves when their feature is absent, and UI Toolkit controllers hide the corresponding HUD and settings rows. Add future genre-exclusive mechanics as a new `OptionalGameFeature` flag instead of checking scene names or concrete player types.

## Game flow and endings

`GameFlowManager` owns transitions, pause state and results, but no presentation, audio or rendering.

```csharp
GameFlowManager.EnsureInstance().StartNewGame();
GameFlowManager.EnsureInstance().ReturnToMainMenu();
GameFlowManager.EnsureInstance().CompleteGame(myEndingDefinition);
GameFlowManager.EnsureInstance().FailGame(myEndingDefinition);
```

An `ObjectiveSet` defines the room objectives and completion ending. Objectives can react to puzzle, item, note and interaction events, or custom code can call:

```csharp
ObjectiveManager.Instance.CompleteObjective("restore_power");
```

For a simple exit or defeat volume, use `GameEndTrigger`.

## Inventory model

Storage and quick access are independent. `InventoryManager.Slots` is persistent storage; `GetQuickSlot` resolves a shortcut without duplicating an item; `AssignQuickSlot` changes only that shortcut. Version 1 saves migrate automatically.

`InventoryItemData` defines category, primary action, readable content, examination, dropping and combinations. UI labels and available actions derive from those capabilities.

Targets implement `IInventoryItemTarget`. `OfferCompatible` opens a filtered selector and never auto-solves. `SelectedOnly` preserves a harder classic mode. `AutoUseSingle` is an explicit accessibility option.

```csharp
public ItemUsePolicy UsePolicy => ItemUsePolicy.OfferCompatible;
public bool ConsumeItemOnUse => true;
public bool AcceptsItem(InventoryItemData item) => item.ItemId == requiredId;
public bool TryUseItem(InventoryItemData item) { Unlock(); return true; }
```

## PC and VR adapters

Gameplay code queries `PlayerPlatformRegistry.Current` instead of `Camera.main` or vendor APIs. It exposes the head, both hands and optional haptics.

The generated VR prefab contains `ModelSocket` children under its hands/controllers. Replace their model children without moving scripts on the rig root.

Unity 6.4 native world-space UI Toolkit is used for VR. `VRUIToolkitPresenter` clones PanelSettings at runtime, so the same prefabs and UXML remain screen-space on PC and world-space in VR. No Canvas dependency is introduced.

## Input and saves

`InputRouter` is the only gameplay input source. Add or rebind actions in `Resources/Input/EscapeRoomInputActions.inputactions`; PC, gamepad and XR bindings coexist without branching mechanics.

The runtime settings menu exposes the principal keyboard bindings through `InputRouter.StartInteractiveRebind`. Overrides are serialized in `GameSettingsData.bindingOverridesJson`, independently from game saves. Call `ResetBindingOverrides` to restore the authored defaults. Gamepad and XR defaults remain in the Input Actions asset so platform bindings cannot be overwritten accidentally by the keyboard screen.

## Advanced evasion

`OptionalGameFeature.AdvancedEvasion` is enabled by the Survival Horror preset, disabled by Escape Room and optional in Custom Hybrid. `Bootstrapper` adds `EvasionController` to the active player only when needed.

PC defaults are `Left Alt + A/D` for collision-safe lean, `X` for look-back and `V` while sprinting forward for slide. All three keyboard actions appear in the runtime rebind screen. In VR, lean/look-back use physical head tracking; artificial slide requires an explicit `VRComfortSettings.allowArtificialSlide` opt-in.

```csharp
EvasionController evasion = GetComponent<EvasionController>();
evasion.TryStartSlide(transform.forward);
evasion.SetLeanOverride(-1f);
evasion.SetLookBackOverride(true);
```

Subscribe to `SlideStarted`, `SlideCompleted` and `SlideCancelled` to add presentation without coupling it to movement logic. `PlayerMovement.CanStand()` and the independent evasion crouch owner prevent standing inside geometry or releasing a hiding-spot crouch.

Keep `SaveId` and `ItemId` stable after publication. Run `Validation/Validate Save IDs` and the smoke tests before packaging an update.
