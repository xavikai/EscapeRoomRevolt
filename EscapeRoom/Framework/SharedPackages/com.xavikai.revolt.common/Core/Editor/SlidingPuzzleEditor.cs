using UnityEditor;
using UnityEngine;
using EscapeRoomRevolt.Systems.Puzzle;

namespace EscapeRoomRevolt.EditorTools
{
    /// <summary>
    /// Draws the solved arrangement as the grid it actually is, instead of the flat row-major list
    /// the data happens to be. A designer should be able to see the answer and click the cell that
    /// stays empty; working out that index 4 of a six-entry list is the middle of the bottom row is
    /// not puzzle design, it is bookkeeping.
    /// </summary>
    [CustomEditor(typeof(SlidingPuzzle))]
    internal sealed class SlidingPuzzleEditor : UnityEditor.Editor
    {
        private const int MaxDrawnCells = 144;

        private SerializedProperty _columns;
        private SerializedProperty _rows;
        private SerializedProperty _holeCell;
        private SerializedProperty _shuffleMoveCount;
        private SerializedProperty _customTargetOrder;
        private SerializedProperty _targetOrder;

        private bool _advancedOpen;

        private void OnEnable()
        {
            _columns = serializedObject.FindProperty("_columns");
            _rows = serializedObject.FindProperty("_rows");
            _holeCell = serializedObject.FindProperty("_holeCell");
            _shuffleMoveCount = serializedObject.FindProperty("_shuffleMoveCount");
            _customTargetOrder = serializedObject.FindProperty("_customTargetOrder");
            _targetOrder = serializedObject.FindProperty("_targetOrder");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, "m_Script",
                "_columns", "_rows", "_holeCell", "_shuffleMoveCount", "_customTargetOrder", "_targetOrder");

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Grid", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_columns);
            EditorGUILayout.PropertyField(_rows);
            EditorGUILayout.PropertyField(_shuffleMoveCount);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Solved arrangement", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(_customTargetOrder.boolValue
                ? "Type each cell's tile id. Leave exactly one cell empty — that is the open slot."
                : "This is what the board looks like when solved. Click a cell to move the open slot there.",
                MessageType.None);

            DrawGrid();

            EditorGUILayout.Space(4);
            _advancedOpen = EditorGUILayout.Foldout(_advancedOpen, "Advanced", true);
            if (_advancedOpen)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_customTargetOrder, new GUIContent("Custom Target Order"));
                if (_customTargetOrder.boolValue)
                    EditorGUILayout.HelpBox(
                        "Only needed when the solution is not the natural reading order — for example tiles carrying "
                        + "symbols that must end up in a pattern. An image board never needs this.",
                        MessageType.Info);
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGrid()
        {
            int columns = Mathf.Max(1, _columns.intValue);
            int rows = Mathf.Max(1, _rows.intValue);

            if (columns * rows > MaxDrawnCells)
            {
                EditorGUILayout.HelpBox($"{columns}x{rows} is too large to preview here.", MessageType.Warning);
                return;
            }
            if (_targetOrder.arraySize != columns * rows)
            {
                // OnValidate resizes the list; until it has run there is nothing coherent to draw.
                EditorGUILayout.HelpBox("Resizing the grid…", MessageType.None);
                return;
            }

            bool custom = _customTargetOrder.boolValue;
            Color previousBackground = GUI.backgroundColor;

            for (int row = 0; row < rows; row++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                for (int column = 0; column < columns; column++)
                {
                    SerializedProperty cell = _targetOrder.GetArrayElementAtIndex(row * columns + column);
                    bool isHole = string.IsNullOrEmpty(cell.stringValue);

                    if (custom)
                    {
                        cell.stringValue = EditorGUILayout.TextField(cell.stringValue,
                            GUILayout.Width(38f), GUILayout.Height(22f));
                        continue;
                    }

                    GUI.backgroundColor = isHole ? new Color(1f, .62f, .25f) : previousBackground;
                    if (GUILayout.Button(isHole ? "—" : cell.stringValue, GUILayout.Width(38f), GUILayout.Height(26f)))
                        _holeCell.vector2IntValue = new Vector2Int(column, row);
                    GUI.backgroundColor = previousBackground;
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            if (!custom) return;

            int holes = 0;
            for (int i = 0; i < _targetOrder.arraySize; i++)
                if (string.IsNullOrEmpty(_targetOrder.GetArrayElementAtIndex(i).stringValue)) holes++;
            if (holes != 1)
                EditorGUILayout.HelpBox($"A sliding puzzle needs exactly one open slot; this grid has {holes}. "
                    + "It will be regenerated from the grid until that is fixed.", MessageType.Error);
        }
    }
}
