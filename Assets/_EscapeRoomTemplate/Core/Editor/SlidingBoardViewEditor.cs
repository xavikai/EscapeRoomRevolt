using UnityEditor;
using UnityEngine;
using EscapeRoomRevolt.Systems.Puzzle;

namespace EscapeRoomRevolt.EditorTools
{
    /// <summary>
    /// Surfaces the one thing that silently breaks a sliding board — a tile count that no longer
    /// matches the grid — and puts rebuilding it one visible click away. The previous version of
    /// this workflow lived in a right-click context menu, which meant resizing a grid appeared to
    /// do nothing at all.
    /// </summary>
    [CustomEditor(typeof(SlidingBoardView))]
    internal sealed class SlidingBoardViewEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var view = (SlidingBoardView)target;
            SlidingPuzzle puzzle = view.Puzzle;

            EditorGUILayout.Space(8);

            if (puzzle == null)
            {
                EditorGUILayout.HelpBox("Assign the puzzle this board renders.", MessageType.Warning);
                return;
            }

            int needed = Mathf.Max(0, puzzle.Columns * puzzle.Rows - 1);
            if (view.TileCount != needed)
                EditorGUILayout.HelpBox(
                    $"The {puzzle.Columns}x{puzzle.Rows} grid needs {needed} tiles and the board has {view.TileCount}. "
                    + "Rebuild to match.", MessageType.Warning);

            float boardWidth = puzzle.Columns * view.CellSize;
            float boardHeight = puzzle.Rows * view.CellSize;
            EditorGUILayout.LabelField("Board size",
                $"{boardWidth:0.00} x {boardHeight:0.00} m  ·  tiles of {view.TileSize:0.00} m");

            if (GUILayout.Button("Rebuild board", GUILayout.Height(26f)))
            {
                int built = SlidingBoardBuilder.Rebuild(view);
                Debug.Log($"[SlidingBoardView] Board rebuilt for {puzzle.Columns}x{puzzle.Rows}: {built} clickable tiles.", view);
            }

            if (GUILayout.Button("Re-apply layout and image"))
            {
                view.ApplyImage();
                view.SnapAll();
            }
        }
    }
}
