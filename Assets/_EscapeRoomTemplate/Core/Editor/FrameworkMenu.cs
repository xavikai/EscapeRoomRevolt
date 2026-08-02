#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using EscapeRoomRevolt.Core;
using EscapeRoomRevolt.Player.PC;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EscapeRoomRevolt.EditorTools
{
    /// <summary>Safe, non-destructive entry points for buyers of the framework.</summary>
    public static class FrameworkMenu
    {
        private const string Root = "Escape Room Framework/";
        private const string GameManagerPrefab = "Assets/_EscapeRoomTemplate/Prefabs/GameManager.prefab";
        private const string PcPlayerPrefab = "Assets/_EscapeRoomTemplate/Prefabs/Player_PC.prefab";
        private const string MainMenuScene = "Assets/_EscapeRoomTemplate/Scenes/MainMenu.unity";
        private const string ShowcaseScene = "Assets/_EscapeRoomTemplate/Scenes/ShowcaseMuseum.unity";
        private const string ExampleScene = "Assets/_EscapeRoomTemplate/Scenes/LockedOffice.unity";
        private const string ManualPath = "Assets/_EscapeRoomTemplate/UserManual.md";
        private const string ProgrammingGuidePath = "Assets/_EscapeRoomTemplate/PROGRAMMING_GUIDE.md";
        private const string CompleteDocumentationPath = "Assets/_EscapeRoomTemplate/DOCUMENTACIO_COMPLETA.md";

        [MenuItem(Root + "Setup/Instantiate Game Manager", priority = 10)]
        public static void InstantiateGameManager()
        {
            Bootstrapper existing = Object.FindAnyObjectByType<Bootstrapper>(FindObjectsInactive.Include);
            if (existing != null)
            {
                SelectAndPing(existing.gameObject);
                Debug.Log("[Escape Room Framework] The active scene already has a Game Manager.", existing);
                return;
            }

            InstantiatePrefabSafely(GameManagerPrefab, "Game Manager");
        }

        [MenuItem(Root + "Setup/Instantiate PC Player", priority = 11)]
        public static void InstantiatePcPlayer()
        {
            PlayerMovement existing = Object.FindAnyObjectByType<PlayerMovement>(FindObjectsInactive.Include);
            if (existing != null)
            {
                SelectAndPing(existing.gameObject);
                Debug.Log("[Escape Room Framework] The active scene already has a PC player.", existing);
                return;
            }

            InstantiatePrefabSafely(PcPlayerPrefab, "PC Player");
        }

        [MenuItem(Root + "Demo/Open Main Menu", priority = 400)]
        public static void OpenMainMenu() => OpenSceneSafely(MainMenuScene);

        [MenuItem(Root + "Demo/Open Showcase Museum", priority = 401)]
        public static void OpenShowcase() => OpenSceneSafely(ShowcaseScene);

        [MenuItem(Root + "Demo/Open Locked Office", priority = 402)]
        public static void OpenExampleRoom() => OpenSceneSafely(ExampleScene);

        [MenuItem(Root + "Maintenance/Preview Missing Scripts", priority = 801)]
        public static void PreviewMissingScripts()
        {
            List<GameObject> affected = FindObjectsWithMissingScripts();
            if (affected.Count == 0)
            {
                EditorUtility.DisplayDialog("Missing Scripts", "No missing scripts were found in the active scene.", "OK");
                return;
            }

            Selection.objects = affected.Cast<Object>().ToArray();
            string preview = string.Join("\n", affected.Take(12).Select(GetHierarchyPath));
            if (affected.Count > 12) preview += $"\n… and {affected.Count - 12} more.";
            EditorUtility.DisplayDialog("Missing Scripts Found", $"Affected objects: {affected.Count}\n\n{preview}\n\nThe objects have been selected. Nothing was modified.", "OK");
        }

        [MenuItem(Root + "Maintenance/Repair Missing Scripts…", priority = 802)]
        public static void RepairMissingScripts()
        {
            List<GameObject> affected = FindObjectsWithMissingScripts();
            if (affected.Count == 0)
            {
                EditorUtility.DisplayDialog("Repair Missing Scripts", "No missing scripts were found in the active scene.", "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog(
                    "Repair Missing Scripts",
                    $"Remove missing MonoBehaviour references from {affected.Count} object(s)?\n\nAn Undo snapshot will be created first.",
                    "Repair",
                    "Cancel"))
                return;

            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
                Undo.RegisterFullObjectHierarchyUndo(root, "Repair Missing Scripts");

            int removed = 0;
            foreach (GameObject gameObject in affected)
                removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log($"[Escape Room Framework] Removed {removed} missing script reference(s) from {affected.Count} object(s). Undo is available.");
        }

        [MenuItem(Root + "Documentation/Open User Manual", priority = 901)]
        public static void OpenManual()
        {
            OpenDocumentation(ManualPath, "User Manual");
        }

        [MenuItem(Root + "Documentation/Open Programming Guide", priority = 903)]
        public static void OpenProgrammingGuide()
        {
            OpenDocumentation(ProgrammingGuidePath, "Programming Guide");
        }

        [MenuItem(Root + "Documentation/Open Complete Documentation", priority = 904)]
        public static void OpenCompleteDocumentation()
        {
            OpenDocumentation(CompleteDocumentationPath, "Complete Documentation");
        }

        private static void InstantiatePrefabSafely(string path, string undoName)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogError($"[Escape Room Framework] Required prefab not found: {path}");
                return;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null) return;
            Undo.RegisterCreatedObjectUndo(instance, "Instantiate " + undoName);
            SelectAndPing(instance);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static void OpenSceneSafely(string path)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        }

        private static void OpenDocumentation(string path, string title)
        {
            Object document = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (document == null)
            {
                Debug.LogError($"[Escape Room Framework] {title} not found at '{path}'.");
                return;
            }

            AssetDatabase.OpenAsset(document);
        }

        private static List<GameObject> FindObjectsWithMissingScripts()
        {
            var result = new List<GameObject>();
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
                foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject) > 0)
                        result.Add(transform.gameObject);
            return result;
        }

        private static string GetHierarchyPath(GameObject gameObject)
        {
            string path = gameObject.name;
            Transform parent = gameObject.transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path;
        }

        private static void SelectAndPing(GameObject gameObject)
        {
            Selection.activeGameObject = gameObject;
            EditorGUIUtility.PingObject(gameObject);
        }
    }
}
#endif
