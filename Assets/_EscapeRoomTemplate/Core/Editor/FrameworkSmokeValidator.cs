#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EscapeRoomRevolt.Systems.Inventory;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEditor.XR.Management;
using UnityEngine.XR.OpenXR;

namespace EscapeRoomRevolt.EditorTools
{
    public static class FrameworkSmokeValidator
    {
        private const string Root = "Escape Room Framework/";

        [MenuItem(Root + "Validation/Run Framework Smoke Tests", priority = 700)]
        public static void Run()
        {
            var failures = new List<string>();
            var warnings = new List<string>();
            RequireAsset<VisualTreeAsset>("Assets/_EscapeRoomTemplate/UI/Toolkit/EscapeRoomMenu.uxml", failures);
            RequireAsset<VisualTreeAsset>("Assets/_EscapeRoomTemplate/UI/Toolkit/GameplayHUD.uxml", failures);
            RequireAsset<PanelSettings>("Assets/_EscapeRoomTemplate/UI/Toolkit/EscapeRoomPanelSettings.asset", failures);
            RequireAsset<GameObject>("Assets/_EscapeRoomTemplate/Prefabs/GameManager.prefab", failures);
            RequireAsset<GameObject>("Assets/_EscapeRoomTemplate/Prefabs/Player_PC.prefab", failures);
            RequireAsset<GameObject>("Assets/_EscapeRoomTemplate/Prefabs/Player_VR.prefab", failures);
            RequireAsset<SceneAsset>("Assets/_EscapeRoomTemplate/Scenes/VRTemplate.unity", failures);
            RequireAsset<EscapeRoomRevolt.Player.VR.VRComfortSettings>(
                "Assets/_EscapeRoomTemplate/Resources/VRComfortSettings.asset", failures);

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/_EscapeRoomTemplate/Scenes/MainMenu.unity") == null)
                warnings.Add("MainMenu.unity has not been generated yet. Run Setup/Create or Update Main Menu Scene.");
            ValidateInputActions(failures);
            ValidateItemIds(failures);
            ValidatePackages(warnings);
            ValidateOpenXR(failures);
            foreach (string warning in warnings) Debug.LogWarning("[Framework Smoke Test] " + warning);
            if (failures.Count == 0)
            {
                Debug.Log($"[Framework Smoke Test] PASS ({warnings.Count} warning(s)).");
                return;
            }
            foreach (string failure in failures) Debug.LogError("[Framework Smoke Test] " + failure);
            Debug.LogError($"[Framework Smoke Test] FAIL ({failures.Count} error(s), {warnings.Count} warning(s)).");
        }

        private static void ValidateInputActions(List<string> failures)
        {
            InputActionAsset actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                "Assets/_EscapeRoomTemplate/Resources/Input/EscapeRoomInputActions.inputactions");
            if (actions == null) { failures.Add("Input Actions asset is missing or was not imported by the Input System."); return; }
            try
            {
                string[] required = { "Move", "Look", "Interact", "Inventory", "Pause", "QuickNavigate", "ToggleFlashlight" };
                foreach (string action in required)
                    if (actions.FindAction("Gameplay/" + action, false) == null) failures.Add($"Gameplay/{action} action is missing.");
            }
            catch (Exception exception) { failures.Add("Input Actions asset is invalid: " + exception.Message); }
        }

        private static void ValidateItemIds(List<string> failures)
        {
            InventoryItemData[] items = AssetDatabase.FindAssets("t:InventoryItemData")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<InventoryItemData>)
                .Where(item => item != null).ToArray();
            foreach (IGrouping<string, InventoryItemData> duplicate in items.GroupBy(item => item.ItemId).Where(group => group.Count() > 1))
                failures.Add($"Duplicate inventory ItemId '{duplicate.Key}': {string.Join(", ", duplicate.Select(item => item.name))}.");
        }

        private static void ValidatePackages(List<string> warnings)
        {
            string manifestPath = Path.GetFullPath("Packages/manifest.json");
            string manifest = File.Exists(manifestPath) ? File.ReadAllText(manifestPath) : string.Empty;
            string[] packages = { "com.unity.xr.interaction.toolkit", "com.unity.xr.management", "com.unity.xr.openxr" };
            foreach (string package in packages)
                if (!manifest.Contains(package, StringComparison.Ordinal)) warnings.Add($"Optional VR dependency '{package}' is not installed.");
        }

        private static void ValidateOpenXR(List<string> failures)
        {
            foreach (BuildTargetGroup target in new[] { BuildTargetGroup.Standalone, BuildTargetGroup.Android })
            {
                var settings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(target);
                bool openXrAssigned = settings != null
                    && settings.InitManagerOnStart
                    && settings.Manager != null
                    && settings.Manager.activeLoaders.Any(loader =>
                        loader != null && loader.GetType().FullName == "UnityEngine.XR.OpenXR.OpenXRLoader");
                if (!openXrAssigned)
                    failures.Add($"OpenXR is not configured to initialize for {target}. Run Setup/Configure OpenXR.");

                OpenXRSettings openXr = OpenXRSettings.GetSettingsForBuildTargetGroup(target);
                bool hasInteractionProfile = openXr != null && openXr.GetFeatures()
                    .Any(feature => feature.enabled && feature.GetType().Name.EndsWith("ControllerProfile", StringComparison.Ordinal));
                if (!hasInteractionProfile)
                    failures.Add($"OpenXR has no enabled controller interaction profile for {target}.");
            }
        }

        private static void RequireAsset<T>(string path, List<string> failures) where T : UnityEngine.Object
        {
            if (AssetDatabase.LoadAssetAtPath<T>(path) == null) failures.Add($"Required asset is missing: {path}");
        }
    }
}
#endif
