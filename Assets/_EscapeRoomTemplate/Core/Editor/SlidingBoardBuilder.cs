using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using EscapeRoomRevolt.Systems.Interaction;
using EscapeRoomRevolt.Systems.Puzzle;

namespace EscapeRoomRevolt.EditorTools
{
    /// <summary>
    /// Builds the physical tiles of a sliding board from the puzzle's grid, so resizing the grid is
    /// a single number rather than a manual round of duplicating cubes and re-typing ids. Shared by
    /// the creation menu and the board's own Inspector button, which is why it lives apart from
    /// both: a "rebuild" that produced different objects from the ones "create" produced would be
    /// worse than no rebuild at all.
    ///
    /// Tiles already on the board are reused and only re-labelled, so art, materials and hand-made
    /// wiring survive a resize; only genuinely surplus tiles are destroyed.
    /// </summary>
    internal static class SlidingBoardBuilder
    {
        private static readonly Color[] Palette =
        {
            new Color(.80f, .25f, .25f), new Color(.90f, .55f, .15f), new Color(.85f, .80f, .25f),
            new Color(.30f, .70f, .35f), new Color(.25f, .45f, .85f), new Color(.55f, .35f, .75f),
            new Color(.25f, .70f, .70f), new Color(.75f, .40f, .55f),
        };

        /// <summary>Rebuilds the board's tiles and empty-slot marker to match the puzzle's grid. Returns how many tiles the board ends up with.</summary>
        public static int Rebuild(SlidingBoardView view)
        {
            SlidingPuzzle puzzle = view != null ? view.Puzzle : null;
            if (puzzle == null)
            {
                Debug.LogWarning("[SlidingBoardBuilder] The board has no puzzle assigned.", view);
                return 0;
            }

            // A rebuild now creates, reparents, rewires and restructures pieces. Grouping it means one
            // Ctrl+Z puts the board back exactly as it was, instead of a dozen half-undone steps.
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Rebuild sliding board");
            int undoGroup = Undo.GetCurrentGroup();

            Transform origin = view.BoardOrigin;
            List<string> ids = new List<string>();
            foreach (string id in puzzle.TargetOrder)
                if (!string.IsNullOrEmpty(id)) ids.Add(id);

            var viewSo = new SerializedObject(view);
            SerializedProperty tiles = viewSo.FindProperty("_tiles");

            // Whatever is already bound, keyed by id, so a resize keeps the tiles a designer dressed.
            var existing = new List<Transform>();
            for (int i = 0; i < tiles.arraySize; i++)
            {
                Object bound = tiles.GetArrayElementAtIndex(i).FindPropertyRelative("tile").objectReferenceValue;
                if (bound is Transform t && t != null) existing.Add(t);
            }

            float tileSize = view.TileSize;
            Vector3 tileScale = new Vector3(tileSize, tileSize, view.Thickness);
            int migrated = 0;

            tiles.arraySize = ids.Count;
            for (int i = 0; i < ids.Count; i++)
            {
                string id = ids[i];
                Transform tile = i < existing.Count ? existing[i] : null;
                if (tile == null) tile = CreateTile(view, origin, tileScale, i);

                // Reused tiles predate the Logic/Visuals split, and leaving them as they are would
                // produce a board that is half old shape and half new. The conversion preserves the
                // art rather than replacing it, so there is nothing to lose by always doing it.
                if (LogicVisualsMigrator.Split(tile.gameObject, out _)) migrated++;

                RenamePiece(tile, "Tile" + id);
                if (tile.parent != origin) Undo.SetTransformParent(tile, origin, "Rebuild sliding board");

                WireTile(tile.gameObject, puzzle, id);

                SerializedProperty binding = tiles.GetArrayElementAtIndex(i);
                binding.FindPropertyRelative("tileId").stringValue = id;
                binding.FindPropertyRelative("tile").objectReferenceValue = tile;
            }

            // Anything the smaller grid no longer has a cell for.
            for (int i = ids.Count; i < existing.Count; i++)
                if (existing[i] != null) Undo.DestroyObjectImmediate(existing[i].gameObject);

            if (EnsureEmptyMarker(view, viewSo, origin, tileSize)) migrated++;
            viewSo.ApplyModifiedProperties();

            view.InvalidatePieces();
            view.ApplyImage();
            view.SnapAll();
            EditorUtility.SetDirty(view);
            Undo.CollapseUndoOperations(undoGroup);

            if (migrated > 0)
                Debug.Log($"[SlidingBoardBuilder] Converted {migrated} older piece(s) to the Logic/Visuals split "
                    + "while rebuilding; their meshes moved to '_Visuals' children and are now replaceable.", view);
            return ids.Count;
        }

        private static Transform CreateTile(SlidingBoardView view, Transform origin, Vector3 size, int paletteIndex)
        {
            GameObject tile;
            if (view.TilePrefab != null)
            {
                // Left at the size its author gave it; the board only lays prefab tiles out, and
                // Drive Tile Scale can be turned off when the art carries its own proportions.
                tile = (GameObject)PrefabUtility.InstantiatePrefab(view.TilePrefab, origin);
            }
            else
            {
                tile = PuzzleCreator.CreatePiece("Tile", origin, size, Palette[paletteIndex % Palette.Length]);
            }

            Undo.RegisterCreatedObjectUndo(tile, "Rebuild sliding board");
            return tile.transform;
        }

        /// <summary>
        /// Renames a piece's logic root and its placeholder together. They are a pair: the visual is
        /// resolved by name, so renaming only the root quietly severs it.
        /// </summary>
        private static void RenamePiece(Transform logic, string baseName)
        {
            Transform visuals = PuzzleCreator.FindVisuals(logic);
            logic.gameObject.name = baseName + "_Logic";
            if (visuals != logic) visuals.gameObject.name = baseName + "_Visuals";
        }

        /// <summary>
        /// Makes a tile actually playable: a relay that knows which id it is, and a click wired to it
        /// on PC and in VR alike. Existing components are re-pointed rather than replaced, and a
        /// click that is already wired is left alone so rebuilding twice does not double every move.
        /// </summary>
        private static void WireTile(GameObject tile, SlidingPuzzle puzzle, string id)
        {
            var button = tile.GetComponent<SlidingTileButton>();
            if (button == null) button = Undo.AddComponent<SlidingTileButton>(tile);

            var buttonSo = new SerializedObject(button);
            buttonSo.FindProperty("_puzzle").objectReferenceValue = puzzle;
            buttonSo.FindProperty("_tileId").stringValue = id;
            buttonSo.ApplyModifiedProperties();

            var trigger = tile.GetComponent<InteractableTrigger>();
            if (trigger == null)
            {
                PuzzleCreator.MakeClickable(tile, "Moure peça", button.Move);
                return;
            }

            for (int i = 0; i < trigger.OnInteractEvent.GetPersistentEventCount(); i++)
                if (trigger.OnInteractEvent.GetPersistentTarget(i) == button) return;

            UnityEventTools.AddVoidPersistentListener(trigger.OnInteractEvent, button.Move);
            EditorUtility.SetDirty(trigger);
        }

        /// <summary>Returns whether an existing marker had to be converted to the Logic/Visuals split.</summary>
        private static bool EnsureEmptyMarker(SlidingBoardView view, SerializedObject viewSo, Transform origin, float tileSize)
        {
            SerializedProperty markerProperty = viewSo.FindProperty("_emptyMarker");
            if (markerProperty.objectReferenceValue is Transform existing && existing != null)
            {
                if (existing.parent != origin) Undo.SetTransformParent(existing, origin, "Rebuild sliding board");
                bool converted = LogicVisualsMigrator.Split(existing.gameObject, out _);
                RenamePiece(existing, "EmptySlot");
                return converted;
            }

            // No collider: the open slot is a hole, not a piece, and it must not eat clicks aimed at
            // the tiles around it.
            GameObject marker = PuzzleCreator.CreatePiece("EmptySlot", origin,
                new Vector3(tileSize, tileSize, view.Thickness * .4f), new Color(.07f, .07f, .08f),
                PuzzleCreator.PieceCollider.None);
            Undo.RegisterCreatedObjectUndo(marker, "Rebuild sliding board");
            markerProperty.objectReferenceValue = marker.transform;
            return false;
        }
    }
}
