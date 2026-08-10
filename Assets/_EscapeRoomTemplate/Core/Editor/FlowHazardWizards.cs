using EscapeRoomRevolt.Systems.Flow;
using UnityEditor;
using UnityEngine;

namespace EscapeRoomRevolt.EditorTools
{
    internal enum MovingHazardDirectionPreset
    {
        WallForward,
        WallBackward,
        CeilingDown,
        FloorUp,
        SlideLeft,
        SlideRight,
        Custom
    }

    /// <summary>Creates a pure movement hazard with an explicit direction chosen before authoring.</summary>
    internal sealed class MovingHazardWizard : EditorWindow
    {
        private MovingHazardDirectionPreset _preset = MovingHazardDirectionPreset.CeilingDown;
        private Vector3 _customDirection = Vector3.down;
        private Vector3 _size = new Vector3(4.5f, .25f, 4.5f);
        private float _distance = 3f;
        private float _travelDuration = 20f;
        private bool _failAtDestination = true;
        private bool _failOnPlayerContact = true;

        public static void Open()
        {
            var window = GetWindow<MovingHazardWizard>(true, "Moving Hazard", true);
            window.minSize = new Vector2(430f, 355f);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Moving hazard — independent movement", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Choose any direction. The generated StartPoint and EndPoint remain editable in the Scene view, so the same mechanic can become a wall, ceiling, floor, platform or water volume.",
                MessageType.Info);

            EditorGUI.BeginChangeCheck();
            MovingHazardDirectionPreset preset = (MovingHazardDirectionPreset)EditorGUILayout.EnumPopup("Direction preset", _preset);
            if (EditorGUI.EndChangeCheck())
            {
                _preset = preset;
                _size = DefaultSize(_preset);
            }

            if (_preset == MovingHazardDirectionPreset.Custom)
                _customDirection = EditorGUILayout.Vector3Field("Custom direction", _customDirection);

            _distance = Mathf.Max(.1f, EditorGUILayout.FloatField("Travel distance", _distance));
            _travelDuration = Mathf.Max(.01f, EditorGUILayout.FloatField("Travel duration", _travelDuration));
            _size = EditorGUILayout.Vector3Field("Hazard size", _size);
            _size = new Vector3(Mathf.Max(.05f, _size.x), Mathf.Max(.05f, _size.y), Mathf.Max(.05f, _size.z));
            _failAtDestination = EditorGUILayout.Toggle("Fail at destination", _failAtDestination);
            _failOnPlayerContact = EditorGUILayout.Toggle("Fail on player contact", _failOnPlayerContact);

            Vector3 direction = ResolveDirection();
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Resolved direction", direction.ToString("F2"));
            EditorGUILayout.LabelField("Path", $"{-direction * (_distance * .5f)}  →  {direction * (_distance * .5f)}");
            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel", GUILayout.Height(28f))) Close();
            GUI.backgroundColor = new Color(.85f, .38f, .28f);
            if (GUILayout.Button("Create moving hazard", GUILayout.Height(28f)))
            {
                PuzzleCreator.CreateConfiguredMovingHazard(direction, _distance, _travelDuration, _size,
                    _failAtDestination, _failOnPlayerContact);
                Close();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        private Vector3 ResolveDirection()
        {
            Vector3 direction;
            switch (_preset)
            {
                case MovingHazardDirectionPreset.WallForward: direction = Vector3.forward; break;
                case MovingHazardDirectionPreset.WallBackward: direction = Vector3.back; break;
                case MovingHazardDirectionPreset.CeilingDown: direction = Vector3.down; break;
                case MovingHazardDirectionPreset.FloorUp: direction = Vector3.up; break;
                case MovingHazardDirectionPreset.SlideLeft: direction = Vector3.left; break;
                case MovingHazardDirectionPreset.SlideRight: direction = Vector3.right; break;
                default: direction = _customDirection; break;
            }
            return direction.sqrMagnitude > .0001f ? direction.normalized : Vector3.down;
        }

        private static Vector3 DefaultSize(MovingHazardDirectionPreset preset)
        {
            if (preset == MovingHazardDirectionPreset.CeilingDown || preset == MovingHazardDirectionPreset.FloorUp)
                return new Vector3(4.5f, .25f, 4.5f);
            if (preset == MovingHazardDirectionPreset.SlideLeft || preset == MovingHazardDirectionPreset.SlideRight)
                return new Vector3(.25f, 3f, 4.5f);
            return new Vector3(4.5f, 3f, .25f);
        }
    }

    /// <summary>Creates a pure HUD countdown that can fail the game without owning any moving object.</summary>
    internal sealed class GameOverTimerWizard : EditorWindow
    {
        private float _duration = 60f;
        private bool _autoStart;
        private bool _showInHud = true;
        private string _hudLabel = "TEMPS RESTANT";

        public static void Open()
        {
            var window = GetWindow<GameOverTimerWizard>(true, "Game Over Timer", true);
            window.minSize = new Vector2(410f, 275f);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Game Over Timer — independent countdown", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "This mechanic only counts down and updates the shared gameplay HUD. Wire StartTimer to a puzzle, trigger, button or Timeline event.",
                MessageType.Info);

            _duration = Mathf.Max(.01f, EditorGUILayout.FloatField("Duration (seconds)", _duration));
            _autoStart = EditorGUILayout.Toggle("Auto start", _autoStart);
            _showInHud = EditorGUILayout.Toggle("Show in gameplay HUD", _showInHud);
            using (new EditorGUI.DisabledScope(!_showInHud))
                _hudLabel = EditorGUILayout.TextField("HUD label", _hudLabel);

            int preview = Mathf.CeilToInt(_duration);
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("HUD preview", $"{preview / 60:00}:{preview % 60:00}", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel", GUILayout.Height(28f))) Close();
            GUI.backgroundColor = new Color(.82f, .62f, .22f);
            if (GUILayout.Button("Create timer", GUILayout.Height(28f)))
            {
                PuzzleCreator.CreateConfiguredGameOverTimer(_duration, _autoStart, _showInHud, _hudLabel);
                Close();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }
    }
}
