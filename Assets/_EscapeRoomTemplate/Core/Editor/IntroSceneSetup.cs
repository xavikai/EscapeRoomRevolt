using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using EscapeRoomRevolt.Core.Flow;

namespace EscapeRoomRevolt.EditorTools
{
    /// <summary>Creates the intro scene that runs before the main menu: company logo, optional cinematic, then the menu.</summary>
    public static class IntroSceneSetup
    {
        private const string Root = "Escape Room Framework/";
        private const string IntroScenePath = "Assets/_EscapeRoomTemplate/Scenes/Intro.unity";
        private const string PanelSettingsPath = "Assets/_EscapeRoomTemplate/UI/Toolkit/EscapeRoomPanelSettings.asset";
        private const string MainMenuPath = "Assets/_EscapeRoomTemplate/Scenes/MainMenu.unity";

        [MenuItem(Root + "Setup/Create or Update Intro Scene", priority = 21)]
        public static void CreateIntroScene()
        {
            if (System.IO.File.Exists(IntroScenePath) &&
                !EditorUtility.DisplayDialog("Intro scene",
                    "Assets/_EscapeRoomTemplate/Scenes/Intro.unity already exists.\n\nReplace it?",
                    "Replace", "Cancel"))
                return;

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // The camera is what the video step renders onto, so it has to exist even for stills.
            Camera camera = Object.FindFirstObjectByType<Camera>();
            if (camera == null)
            {
                var cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
            }
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;

            var host = new GameObject("IntroSequence");
            var sequence = host.AddComponent<CutsceneSequence>();

            var so = new SerializedObject(sequence);
            so.FindProperty("_playOnStart").boolValue = true;
            so.FindProperty("_skippable").boolValue = true;
            so.FindProperty("_panelSettings").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);

            // One empty image step as a placeholder: drop the company logo into it and it works.
            SerializedProperty steps = so.FindProperty("_steps");
            steps.arraySize = 1;
            SerializedProperty first = steps.GetArrayElementAtIndex(0);
            first.FindPropertyRelative("kind").enumValueIndex = (int)CutsceneStepKind.Image;
            first.FindPropertyRelative("duration").floatValue = 2.5f;
            first.FindPropertyRelative("fadeDuration").floatValue = .6f;
            so.ApplyModifiedProperties();

            // When the sequence ends, go to the menu. Added last: applying a stale SerializedObject
            // afterwards would wipe the listener.
            var loader = host.AddComponent<IntroSceneLoader>();
            UnityEventTools.AddVoidPersistentListener(sequence.OnFinished, loader.LoadMainMenu);

            EditorSceneManager.SaveScene(scene, IntroScenePath);
            RegisterFirstInBuild();

            Selection.activeGameObject = host;
            Debug.Log("[Escape Room Framework] Intro scene created and set first in the Build Profile. "
                + "Select 'IntroSequence' and drop your company logo into Steps > Image. "
                + "Add more steps for a title card, a video file or an in-engine camera shot.", host);
        }

        /// <summary>Puts the intro before every other scene, since the build boots into whatever is first.</summary>
        private static void RegisterFirstInBuild()
        {
            List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
            scenes.RemoveAll(s => s.path == IntroScenePath);
            scenes.Insert(0, new EditorBuildSettingsScene(IntroScenePath, true));

            // The menu must still be in the list, or the intro would have nowhere to go.
            if (!scenes.Any(s => s.path == MainMenuPath) && System.IO.File.Exists(MainMenuPath))
                scenes.Insert(1, new EditorBuildSettingsScene(MainMenuPath, true));

            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
