#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EscapeRoomRevolt.Core.Settings;
using EscapeRoomRevolt.Core.Flow;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace EscapeRoomRevolt.EditorTools
{
    /// <summary>Checks compiled product boundaries and serialized references after module moves.</summary>
    public static class ProductModuleValidator
    {
        [Serializable]
        private sealed class Report
        {
            public string unityVersion;
            public string result;
            public string productId;
            public string firstGameplayScene;
            public string persistentDataDirectory;
            public bool survivalAssemblyPresent;
            public List<string> sharedAssemblies = new();
            public List<string> scenes = new();
            public int prefabs;
            public List<string> issues = new();
        }

        [MenuItem("Escape Room Framework/Validation/Validate Product Module Boundaries", priority = 702)]
        public static void ValidateFromMenu()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) ValidateBatch();
        }

        // Command line: -batchmode -quit -executeMethod EscapeRoomRevolt.EditorTools.ProductModuleValidator.ValidateBatch
        public static void ValidateBatch()
        {
            var report = new Report { unityVersion = Application.unityVersion };
            var textSettings = Resources.Load<TMPro.TMP_Settings>("TMP Settings");
            if (textSettings == null || TMPro.TMP_Settings.defaultFontAsset == null || TMPro.TMP_Settings.defaultStyleSheet == null)
                report.issues.Add("TextMesh Pro Resources defaults (settings, font, style sheet) are incomplete.");
            ProductConfiguration product = ProductConfiguration.Current;
            report.survivalAssemblyPresent = AppDomain.CurrentDomain.GetAssemblies()
                .Any(assembly => assembly.GetName().Name == "EscapeRoomRevolt.Survival");
            if (product != null)
            {
                report.productId = product.ProductId;
                report.persistentDataDirectory = ProductConfiguration.PersistentDataDirectory;
                var flow = Resources.Load<GameFlowSettings>("GameFlowSettings");
                report.firstGameplayScene = flow != null ? flow.FirstGameplayScene : null;
                if (flow == null || !product.DesktopScenes.Any(scene => Path.GetFileNameWithoutExtension(scene) == flow.FirstGameplayScene))
                    report.issues.Add("Product gameplay routing is missing or outside its desktop scene list.");
                if (PlayerSettings.productName != product.DisplayName)
                    report.issues.Add("Product name does not match its configuration.");
                bool survival = GameFeatures.Genre == GameGenre.SurvivalHorror;
                if (survival != report.survivalAssemblyPresent)
                    report.issues.Add("Installed Survival assembly does not match the product profile.");
                if (!survival && Resources.Load("SurvivalDifficultySettings") != null)
                    report.issues.Add("Escape Room imports Survival difficulty resources.");
                if (survival && Resources.Load<VisualTreeAsset>("SurvivalHUD") == null)
                    report.issues.Add("Survival overlay template is missing.");
                if (Directory.GetFiles("Assets/_EscapeRoomTemplate", "*.cs", SearchOption.AllDirectories).Length != 0)
                    report.issues.Add("Consumer project contains a duplicated copy of framework source code.");
            }
            string[] shared = { "Core", "Player", "Systems", "UI", "Bootstrap" };
            foreach (string suffix in shared)
            {
                string name = "EscapeRoomRevolt." + suffix;
                var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(candidate => candidate.GetName().Name == name);
                if (assembly == null) { report.issues.Add("Shared assembly not loaded: " + name); continue; }
                report.sharedAssemblies.Add(name);
                foreach (var reference in assembly.GetReferencedAssemblies())
                    if (reference.Name.StartsWith("EscapeRoomRevolt.Survival", StringComparison.Ordinal))
                        report.issues.Add(name + " references optional product assembly " + reference.Name);
            }

            SceneSetup[] previous = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                foreach (EditorBuildSettingsScene entry in EditorBuildSettings.scenes.Where(scene => scene.enabled))
                {
                    Scene scene = EditorSceneManager.OpenScene(entry.path, OpenSceneMode.Single);
                    report.scenes.Add(entry.path);
                    if (scene.rootCount == 0) report.issues.Add("Scene has no root objects: " + entry.path);
                    if (product != null && Path.GetFileNameWithoutExtension(entry.path) == report.firstGameplayScene
                        && !scene.GetRootGameObjects().Any(root => root.GetComponentInChildren<EscapeRoomRevolt.Core.Bootstrapper>(true) != null))
                        report.issues.Add("Product gameplay scene has no Bootstrapper: " + entry.path);
                    foreach (GameObject root in scene.GetRootGameObjects()) CheckHierarchy(root, entry.path, report.issues);
                }
                foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_EscapeRoomTemplate" }))
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    GameObject prefab = EscapeRoomRevolt.EditorTools.FrameworkAssetPaths.Load<GameObject>(assetPath);
                    if (prefab == null) { report.issues.Add("Could not load prefab: " + assetPath); continue; }
                    report.prefabs++;
                    CheckHierarchy(prefab, assetPath, report.issues);
                }
            }
            finally
            {
                if (previous.Length > 0) EditorSceneManager.RestoreSceneManagerSetup(previous);
                else EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }

            report.result = report.issues.Count == 0 ? "Passed" : "Failed";
            Directory.CreateDirectory(".utmp");
            File.WriteAllText(".utmp/product-module-validation.json", JsonUtility.ToJson(report, true));
            if (report.issues.Count > 0)
                throw new InvalidOperationException("Product module validation failed:\n" + string.Join("\n", report.issues));
            Debug.Log($"[Product Modules] PASS: {report.sharedAssemblies.Count} shared assemblies, {report.scenes.Count} scenes, {report.prefabs} prefabs; no Survival references from shared assemblies or broken serialized references.");
        }

        private static void CheckHierarchy(GameObject root, string location, List<string> issues)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject);
                if (missing > 0) issues.Add($"{location}: {child.name} has {missing} missing scripts.");
                foreach (Component component in child.GetComponents<Component>())
                {
                    if (component == null) continue;
                    var serialized = new SerializedObject(component);
                    SerializedProperty property = serialized.GetIterator();
                    while (property.Next(true))
                        if (property.propertyType == SerializedPropertyType.ObjectReference
                            && property.objectReferenceValue == null && property.objectReferenceInstanceIDValue != 0)
                            issues.Add($"{location}: {child.name}/{component.GetType().Name}.{property.propertyPath} has a broken reference.");
                }
            }
        }
    }
}
#endif
