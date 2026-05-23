# 🔐 EscapeRoomRevolt

A professional, modular and scalable Unity 6 (URP) template for creating escape room games.

## 🎯 Vision

This is not a single closed room — it's a **reusable framework** where rooms, objects and puzzles can be added without rewriting code. Built for PC first-person, with architecture ready for VR.

## 🏗️ Architecture Principles

1. **Separate data, logic and scene** — Configuration via `ScriptableObjects`, logic in independent components
2. **Common interfaces** — `IInteractable` for interactions, `ISaveable` for saving
3. **Shared PC/VR logic** — Puzzles, inventory and saving don't depend on input
4. **State-based saving** — States registered via stable `SaveId` and JSON

## 📁 Project Structure

```
Assets/_EscapeRoomTemplate/
├── Core/
│   ├── Runtime/        # Core framework (EventBus, Bootstrapper, GameContext)
│   ├── Editor/         # Custom editors and tools
│   └── Tests/          # Unit tests
├── Systems/
│   ├── Interaction/    # IInteractable, InteractionManager, PC Raycast
│   ├── Inventory/      # InventoryManager, InventoryItemData (SO)
│   ├── Puzzle/         # PuzzleController, CodePanel, Sequence, Socket puzzles
│   ├── SaveLoad/       # ISaveable, SaveManager, JSON serialization
│   ├── Objectives/     # ObjectiveManager, ObjectiveData
│   ├── Audio/          # Audio management
│   └── Events/         # EventBus, GameEvent system
├── Player/
│   ├── PC/             # PC first-person controller
│   └── VR/             # VR layer (XR Interaction Toolkit)
├── UI/
│   ├── PC/             # Menus, inventory UI, numeric keypads
│   └── VR/             # World Space UI for VR
├── ScriptableObjects/
│   ├── Items/          # InventoryItemData assets
│   ├── Puzzles/        # Puzzle configuration assets
│   ├── Rooms/          # Room definition assets
│   └── Dialogues/      # Dialogue/note assets
├── Scenes/             # Game scenes
├── Prefabs/            # Reusable prefabs
├── Art/                # Visual assets
├── Audio/              # Audio assets
└── Documentation/      # Docs and diagrams
```

## 🎮 Demo: The Locked Office

A vertical slice to validate all systems in a playable loop:
1. Enter the office
2. Find a key (add to inventory)
3. Open a drawer using the key
4. Read a note with a clue (reading system)
5. Enter a code in a safe (numeric puzzle)
6. Get a fuse from the safe
7. Place the fuse in the electrical panel (socket puzzle)
8. Final door opens — demo complete!

## 🗺️ Roadmap

| EPIC | System | Status |
|------|--------|--------|
| 01 | Project Foundation | 🚧 In Progress |
| 02 | Interaction System | ⏳ Pending |
| 03 | Inventory System | ⏳ Pending |
| 04 | Puzzle System | ⏳ Pending |
| 05 | Save & Load | ⏳ Pending |
| 06 | UI | ⏳ Pending |
| 07 | Demo: The Locked Office | ⏳ Pending |
| 08 | VR Layer | ⏳ Pending |

## 🛠️ Requirements

- Unity 6 (URP)
- Input System package
- XR Interaction Toolkit (for VR layer)

---

*Built with ❤️ using Antigravity AI*
