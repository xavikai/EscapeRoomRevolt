using EscapeRoomRevolt.Systems.Puzzle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EscapeRoomRevolt.EditorTools
{
    /// <summary>Lets designers resize and rebuild an existing number-wheels puzzle without touching its events or identity.</summary>
    [CustomEditor(typeof(NumberWheelsPuzzleAuthoring))]
    internal sealed class NumberWheelsPuzzleAuthoringEditor : UnityEditor.Editor
    {
        private SerializedProperty _wheelCount;
        private SerializedProperty _combination;

        private void OnEnable()
        {
            _wheelCount = serializedObject.FindProperty("_wheelCount");
            _combination = serializedObject.FindProperty("_combination");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Dynamic lock layout", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Rebuild changes only the generated lock presentation and StatePuzzle conditions. PuzzleDefinition, hints and OnSolved consequences remain intact. Assigned replacement model prefabs are preserved for wheels that still exist.",
                MessageType.Info);

            EditorGUI.BeginChangeCheck();
            int count = EditorGUILayout.IntSlider("Number of wheels", _wheelCount.intValue,
                NumberWheelsPuzzleAuthoring.MinimumWheels, NumberWheelsPuzzleAuthoring.MaximumWheels);
            if (EditorGUI.EndChangeCheck())
            {
                _wheelCount.intValue = count;
                ResizeCombination(count);
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Solution", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            for (int index = 0; index < _combination.arraySize; index++)
            {
                SerializedProperty digit = _combination.GetArrayElementAtIndex(index);
                EditorGUILayout.BeginVertical(GUILayout.Width(42f));
                EditorGUILayout.LabelField((index + 1).ToString(), EditorStyles.centeredGreyMiniLabel,
                    GUILayout.Width(38f));
                digit.intValue = Mathf.Clamp(EditorGUILayout.IntField(digit.intValue, GUILayout.Width(38f)), 0, 9);
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();

            var authoring = (NumberWheelsPuzzleAuthoring)target;
            float width = PuzzleCreator.CalculateNumberWheelsPanelWidth(authoring.WheelCount);
            float fov = PuzzleCreator.CalculateNumberWheelsFocusFov(authoring.WheelCount);
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Generated panel", $"{width:0.00} m wide · focus FOV {fov:0}°");

            if (GUILayout.Button("Rebuild wheels and layout", GUILayout.Height(28f)))
            {
                int[] digits = new int[authoring.WheelCount];
                for (int index = 0; index < digits.Length; index++) digits[index] = authoring.GetDigit(index);

                Undo.RegisterFullObjectHierarchyUndo(authoring.gameObject, "Rebuild Number Wheels Puzzle");
                PuzzleCreator.RebuildNumberWheelsPuzzleKit(authoring.gameObject, digits);
                EditorSceneManager.MarkSceneDirty(authoring.gameObject.scene);
                Selection.activeGameObject = authoring.gameObject;
            }
        }

        private void ResizeCombination(int count)
        {
            int oldSize = _combination.arraySize;
            _combination.arraySize = count;
            for (int index = oldSize; index < count; index++)
                _combination.GetArrayElementAtIndex(index).intValue = 0;
        }
    }
}
