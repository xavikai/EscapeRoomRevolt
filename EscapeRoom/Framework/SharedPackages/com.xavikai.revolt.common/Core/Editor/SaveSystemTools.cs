using System;
using System.Collections.Generic;
using System.Linq;
using EscapeRoomRevolt.Core.Save;
using EscapeRoomRevolt.Systems.Flow;
using EscapeRoomRevolt.Systems.Interaction;
using EscapeRoomRevolt.Systems.Puzzle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EscapeRoomRevolt.EditorTools
{
    /// <summary>
    /// Keeps the Save IDs of a scene unique. Two objects with the same Save ID share one entry in
    /// a saved game: after loading, the second one copies the state of the first (a closed drawer
    /// opens, a zone that never fired counts as fired). It happens every time an object is
    /// duplicated with Ctrl+D, because the copy keeps the original's ID.
    ///
    /// Duplicates are fixed automatically when the scene is saved, reported when Play starts, and
    /// can be checked at any time from Escape Room Framework > Validation > Validate Save IDs.
    /// </summary>
    [InitializeOnLoad]
    public static class SaveSystemTools
    {
        private const string Tag = "[Save IDs]";

        static SaveSystemTools()
        {
            EditorSceneManager.sceneSaving -= OnSceneSaving;
            EditorSceneManager.sceneSaving += OnSceneSaving;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        [MenuItem("Escape Room Framework/Validation/Validate Save IDs", priority = 702)]
        public static void FixSaveIds()
        {
            int changed = 0;
            int unresolved = 0;
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                Scene scene = SceneManager.GetSceneAt(index);
                if (!scene.isLoaded) continue;
                Result result = Process(scene, apply: true);
                changed += result.Changed;
                unresolved += result.Unresolved;
                if (result.Changed > 0) EditorSceneManager.MarkSceneDirty(scene);
            }

            if (changed > 0)
                Debug.Log($"{Tag} Assigned {changed} new Save ID(s). Save the scene (Ctrl+S) to keep them.");
            if (unresolved == 0 && changed == 0)
                Debug.Log($"{Tag} Every saveable object in the open scenes has a unique Save ID.");
        }

        private static void OnSceneSaving(Scene scene, string path)
        {
            if (Application.isPlaying) return;
            Result result = Process(scene, apply: true);
            if (result.Changed > 0)
                Debug.Log($"{Tag} {scene.name}: gave {result.Changed} duplicated or empty Save ID(s) a new value before saving.");
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.ExitingEditMode) return;
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                Scene scene = SceneManager.GetSceneAt(index);
                if (!scene.isLoaded) continue;
                Result result = Process(scene, apply: false);
                if (result.Changed > 0)
                    Debug.LogWarning($"{Tag} {scene.name}: {result.Changed} object(s) share or lack a Save ID, so saved "
                        + "games will mix up their state. Save the scene (Ctrl+S) or run Escape Room Framework > "
                        + "Validation > Validate Save IDs to fix them.");
            }
        }

        private struct Result
        {
            public int Changed;
            public int Unresolved;
        }

        private static Result Process(Scene scene, bool apply)
        {
            var result = new Result();
            List<MonoBehaviour> saveables = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<MonoBehaviour>(true))
                .Where(behaviour => behaviour is ISaveable)
                .ToList();

            // 1. Objects whose ID is meant to be generated but is still empty.
            foreach (MonoBehaviour behaviour in saveables)
            {
                SerializedProperty property = EditableId(behaviour, out SerializedObject serialized);
                if (property == null || !string.IsNullOrWhiteSpace(property.stringValue) || behaviour is PuzzleController) continue;
                result.Changed++;
                if (apply) Assign(serialized, property, behaviour);
            }

            // 2. Objects that share an ID. The first one in the Hierarchy keeps it.
            IEnumerable<IGrouping<string, MonoBehaviour>> groups = saveables
                .GroupBy(behaviour => ((ISaveable)behaviour).SaveId ?? string.Empty)
                .Where(group => group.Count() > 1);
            foreach (IGrouping<string, MonoBehaviour> group in groups)
            {
                foreach (MonoBehaviour duplicate in group.OrderBy(HierarchyOrder).Skip(1))
                {
                    SerializedProperty property = EditableId(duplicate, out SerializedObject serialized);
                    if (property == null)
                    {
                        // Managers with a fixed ID (player, equipment, flashlight...) are one per scene by
                        // design; only puzzles that share a Puzzle Definition need the student's help.
                        if (duplicate is PuzzleController)
                        {
                            result.Unresolved++;
                            Debug.LogError(DescribeUnresolved(duplicate, group.Key), duplicate);
                        }
                        continue;
                    }

                    result.Changed++;
                    if (apply) Assign(serialized, property, duplicate);
                }
            }

            return result;
        }

        /// <summary>The serialized string field that holds this object's Save ID, if it can be changed safely.</summary>
        private static SerializedProperty EditableId(MonoBehaviour behaviour, out SerializedObject serialized)
        {
            serialized = new SerializedObject(behaviour);
            switch (behaviour)
            {
                case InteractableBase _:
                case EventTriggerZone _:
                case GameOverTimer _:
                case MovingHazard _:
                case TimedGameOverHazard _:
                    return serialized.FindProperty("_saveId");
                case NarrativeTrigger _:
                    return serialized.FindProperty("_guid");
                case PuzzleController puzzle:
                    // A puzzle with a Puzzle Definition takes its ID from the definition's Persistent Id:
                    // that one has to be fixed by giving each puzzle its own definition.
                    return puzzle.Definition != null && !string.IsNullOrWhiteSpace(puzzle.Definition.PersistentId)
                        ? null
                        : serialized.FindProperty("_puzzleId");
                default:
                    return null;
            }
        }

        private static void Assign(SerializedObject serialized, SerializedProperty property, MonoBehaviour behaviour)
        {
            string prefix = behaviour is PuzzleController ? behaviour.name.Replace(' ', '_') + "_" : string.Empty;
            property.stringValue = prefix + Guid.NewGuid().ToString("N");
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(behaviour);
        }

        private static string DescribeUnresolved(MonoBehaviour behaviour, string saveId) =>
            $"{Tag} '{behaviour.name}' uses the same Puzzle Definition (Persistent Id '{saveId}') as another puzzle. "
            + "Give each puzzle its own Puzzle Definition with a different Persistent Id.";

        private static string HierarchyOrder(MonoBehaviour behaviour)
        {
            var path = new List<string>();
            for (Transform current = behaviour.transform; current != null; current = current.parent)
                path.Add(current.GetSiblingIndex().ToString("D5"));
            path.Reverse();
            return string.Join("/", path) + "#" + behaviour.GetComponents<MonoBehaviour>().ToList().IndexOf(behaviour).ToString("D3");
        }
    }
}
