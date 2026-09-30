using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EscapeRoomRevolt.EditorTools
{
    /// <summary>Creation window for a number-wheels puzzle whose wheel count and solution are authored up front.</summary>
    internal sealed class NumberWheelsPuzzleWizard : EditorWindow
    {
        private int _wheelCount = 4;
        private readonly List<int> _digits = new List<int> { 3, 1, 4, 2 };

        public static void Open()
        {
            var window = GetWindow<NumberWheelsPuzzleWizard>(true, "Number Wheels Puzzle", true);
            window.minSize = new Vector2(390f, 245f);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Dynamic number-wheels puzzle", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Choose the number of decimal wheels and the solution. The housing, title, spacing and focus camera are sized automatically.",
                MessageType.Info);

            EditorGUI.BeginChangeCheck();
            int nextCount = EditorGUILayout.IntSlider("Number of wheels", _wheelCount,
                Systems.Puzzle.NumberWheelsPuzzleAuthoring.MinimumWheels,
                Systems.Puzzle.NumberWheelsPuzzleAuthoring.MaximumWheels);
            if (EditorGUI.EndChangeCheck()) ResizeDigits(nextCount);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Solution", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            for (int index = 0; index < _digits.Count; index++)
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(42f));
                EditorGUILayout.LabelField((index + 1).ToString(), EditorStyles.centeredGreyMiniLabel,
                    GUILayout.Width(38f));
                _digits[index] = EditorGUILayout.IntField(_digits[index], GUILayout.Width(38f));
                _digits[index] = Mathf.Clamp(_digits[index], 0, 9);
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Code preview", string.Concat(_digits), EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel", GUILayout.Height(28f))) Close();
            GUI.backgroundColor = new Color(.45f, .8f, .55f);
            if (GUILayout.Button("Create puzzle", GUILayout.Height(28f)))
            {
                PuzzleCreator.CreateConfiguredNumberWheelsPuzzle(_digits.ToArray());
                Close();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        private void ResizeDigits(int count)
        {
            _wheelCount = Mathf.Clamp(count,
                Systems.Puzzle.NumberWheelsPuzzleAuthoring.MinimumWheels,
                Systems.Puzzle.NumberWheelsPuzzleAuthoring.MaximumWheels);
            while (_digits.Count < _wheelCount) _digits.Add(0);
            if (_digits.Count > _wheelCount) _digits.RemoveRange(_wheelCount, _digits.Count - _wheelCount);
        }
    }
}
