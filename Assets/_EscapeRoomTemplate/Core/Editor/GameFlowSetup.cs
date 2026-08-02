#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using EscapeRoomRevolt.Core.Flow;
using EscapeRoomRevolt.UI.Toolkit;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace EscapeRoomRevolt.EditorTools
{
    public static class GameFlowSetup
    {
        private const string Root = "Escape Room Framework/";
        private const string MainMenuPath = "Assets/_EscapeRoomTemplate/Scenes/MainMenu.unity";
        private const string MenuUxmlPath = "Assets/_EscapeRoomTemplate/UI/Toolkit/EscapeRoomMenu.uxml";
        private const string PanelSettingsPath = "Assets/_EscapeRoomTemplate/UI/Toolkit/EscapeRoomPanelSettings.asset";
        private const string ResourcesFolder = "Assets/_EscapeRoomTemplate/Resources";
        private const string SettingsPath = ResourcesFolder + "/GameFlowSettings.asset";

        [MenuItem(Root + "Setup/Create or Update Main Menu Scene...", priority = 20)]
        public static void CreateMainMenuScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuPath) != null
                && !EditorUtility.DisplayDialog(
                    "Update Main Menu",
                    "Rebuild the framework MainMenu scene? The existing scene file will be replaced.",
                    "Rebuild",
                    "Cancel"))
                return;

            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MenuUxmlPath);
            PanelSettings panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            if (visualTree == null || panelSettings == null)
            {
                Debug.LogError("[Escape Room Framework] The UI Toolkit menu assets are missing. MainMenu was not created.");
                return;
            }

            EnsureSettingsAsset();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraRoot = new GameObject("MainMenuCamera");
            Camera menuCamera = cameraRoot.AddComponent<Camera>();
            menuCamera.clearFlags = CameraClearFlags.SolidColor;
            menuCamera.backgroundColor = new Color(0.008f, 0.012f, 0.012f, 1f);
            menuCamera.cullingMask = 0;
            menuCamera.depth = -100f;
            menuCamera.allowHDR = false;
            menuCamera.allowMSAA = false;
            menuCamera.useOcclusionCulling = false;

            var root = new GameObject("MainMenuUI");
            UIDocument document = root.AddComponent<UIDocument>();
            document.panelSettings = panelSettings;
            document.visualTreeAsset = visualTree;
            document.sortingOrder = 100;
            root.AddComponent<UIToolkitMenuController>();

            EditorSceneManager.SaveScene(scene, MainMenuPath);
            EnsureFirstBuildScene(MainMenuPath);
            Selection.activeGameObject = root;
            Debug.Log("[Escape Room Framework] MainMenu scene created and placed first in Build Settings.", root);
        }

        [MenuItem(Root + "Create/Flow/Objective Manager", priority = 250)]
        public static void CreateObjectiveManager()
        {
            ObjectiveManager existing = Object.FindAnyObjectByType<ObjectiveManager>(FindObjectsInactive.Include);
            if (existing != null)
            {
                Selection.activeGameObject = existing.gameObject;
                EditorGUIUtility.PingObject(existing);
                return;
            }

            var gameObject = new GameObject("ObjectiveManager");
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Objective Manager");
            gameObject.AddComponent<ObjectiveManager>();
            Selection.activeGameObject = gameObject;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        [MenuItem(Root + "Create/Flow/Game End Trigger", priority = 251)]
        public static void CreateGameEndTrigger()
        {
            var gameObject = new GameObject("GameEndTrigger");
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Game End Trigger");
            var collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            gameObject.AddComponent<GameEndTrigger>();
            Selection.activeGameObject = gameObject;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static void EnsureSettingsAsset()
        {
            if (AssetDatabase.LoadAssetAtPath<GameFlowSettings>(SettingsPath) != null) return;
            if (!AssetDatabase.IsValidFolder(ResourcesFolder))
                AssetDatabase.CreateFolder("Assets/_EscapeRoomTemplate", "Resources");
            GameFlowSettings settings = ScriptableObject.CreateInstance<GameFlowSettings>();
            AssetDatabase.CreateAsset(settings, SettingsPath);
            AssetDatabase.SaveAssets();
        }

        private static void EnsureFirstBuildScene(string path)
        {
            var scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(path, true)
            };
            scenes.AddRange(EditorBuildSettings.scenes.Where(scene => scene.path != path));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
#endif
