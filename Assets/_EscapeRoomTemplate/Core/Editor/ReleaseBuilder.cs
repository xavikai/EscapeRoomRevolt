#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace EscapeRoomRevolt.EditorTools
{
    /// <summary>Reproducible release builds used for the downloadable PC and Quest packages.</summary>
    public static class ReleaseBuilder
    {
        private const string Version = "v0.1.0-beta.1";
        private const string ReleaseRoot = "Builds/Release/" + Version;

        private static readonly string[] WindowsScenes =
        {
            "Assets/_EscapeRoomTemplate/Scenes/Intro.unity",
            "Assets/_EscapeRoomTemplate/Scenes/MainMenu.unity",
            "Assets/_EscapeRoomTemplate/Scenes/ShowcaseMuseum.unity",
            "Assets/_EscapeRoomTemplate/Scenes/LockedOffice.unity",
            "Assets/_EscapeRoomTemplate/Scenes/SurvivalHorrorDemo.unity"
        };

        private static readonly string[] QuestScenes =
        {
            "Assets/_EscapeRoomTemplate/Scenes/ShowcaseMuseumVR.unity"
        };

        [MenuItem("Escape Room Framework/Build/Release/Build Windows", priority = 900)]
        public static void BuildWindows()
        {
            EnsureVersion();
            string output = Path.GetFullPath(Path.Combine(ReleaseRoot, "Windows", "EscapeRoomRevolt.exe"));
            Directory.CreateDirectory(Path.GetDirectoryName(output) ?? ReleaseRoot);
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = WindowsScenes,
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.None
            };
            BuildOrThrow(options, "Windows");
        }

        [MenuItem("Escape Room Framework/Build/Release/Build Quest APK", priority = 901)]
        public static void BuildQuest()
        {
            EnsureVersion();
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.xavikai.escaperoomrevolt");
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            EditorUserBuildSettings.buildAppBundle = false;

            string output = Path.GetFullPath(Path.Combine(
                ReleaseRoot, "EscapeRoomRevolt-VR-Quest-" + Version + ".apk"));
            Directory.CreateDirectory(Path.GetDirectoryName(output) ?? ReleaseRoot);
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = QuestScenes,
                locationPathName = output,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.None
            };
            BuildOrThrow(options, "Quest");
        }

        private static void EnsureVersion()
        {
            PlayerSettings.companyName = "XaviKai";
            PlayerSettings.productName = "Escape Room Revolt";
            PlayerSettings.bundleVersion = "0.1.0";
            AssetDatabase.SaveAssets();
        }

        private static void BuildOrThrow(BuildPlayerOptions options, string label)
        {
            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;
            if (summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException(
                    $"{label} build failed: {summary.result}, {summary.totalErrors} error(s).");

            Debug.Log($"[Release Build] {label} PASS — {summary.outputPath} ({summary.totalSize} bytes)");
        }
    }
}
#endif
