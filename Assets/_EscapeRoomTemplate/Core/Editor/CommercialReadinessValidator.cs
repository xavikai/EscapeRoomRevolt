#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using EscapeRoomRevolt.Core.Save;
using EscapeRoomRevolt.Core.Settings;
using EscapeRoomRevolt.Systems.Inventory;
using EscapeRoomRevolt.Systems.Puzzle;
using EscapeRoomRevolt.Systems.Survival;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace EscapeRoomRevolt.EditorTools
{
    /// <summary>One-click validation buyers can run before building or packaging a level.</summary>
    public static class CommercialReadinessValidator
    {
        [MenuItem("Escape Room Framework/Validation/Validate Current Scene", priority = 701)]
        public static void ValidateLoadedScene()
        {
            var issues = new List<string>();

            GenreFeatureSettings genreSettings = AssetDatabase.LoadAssetAtPath<GenreFeatureSettings>(
                "Assets/_EscapeRoomTemplate/Resources/GenreFeatureSettings.asset");
            if (genreSettings == null)
                issues.Add("Falta GenreFeatureSettings.asset. Selecciona un perfil desde Escape Room Framework/Configuration.");

            int canvasCount = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include).Length;
            if (canvasCount > 0) issues.Add($"La escena contiene {canvasCount} Canvas heredados.");

            int documents = Object.FindObjectsByType<UIDocument>(FindObjectsInactive.Include).Length;
            if (documents < 2) issues.Add("Faltan los UIDocument de GameplayUI o MenuUI.");

            if (LayerMask.NameToLayer("Interactable") < 0) issues.Add("Falta la capa Interactable.");
            if (LayerMask.NameToLayer("Examine") < 0) issues.Add("Falta la capa Examine.");

            string activeScenePath = SceneManager.GetActiveScene().path;
            string[] buildScenePaths = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();
            if (buildScenePaths.Length == 0) issues.Add("Build Settings no contiene escenas habilitadas.");
            else if (!buildScenePaths.Contains(activeScenePath))
                issues.Add($"La escena activa '{activeScenePath}' no está habilitada en Build Settings.");

            PuzzleController[] puzzles = Object.FindObjectsByType<PuzzleController>(FindObjectsInactive.Include);
            foreach (PuzzleController puzzle in puzzles)
            {
                if (puzzle.Definition == null)
                {
                    issues.Add($"El puzle '{puzzle.name}' no tiene PuzzleDefinition.");
                    continue;
                }

                if (puzzle.Definition.Hints == null)
                    issues.Add($"El puzle '{puzzle.name}' no tiene pistas progresivas.");
            }

            var saveIds = new HashSet<string>();
            foreach (ISaveable saveable in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include).OfType<ISaveable>())
            {
                if (string.IsNullOrWhiteSpace(saveable.SaveId)) issues.Add($"{saveable.GetType().Name} tiene un SaveId vacío.");
                else if (!saveIds.Add(saveable.SaveId)) issues.Add($"SaveId duplicado: '{saveable.SaveId}'.");
            }

            var itemIds = new HashSet<string>();
            string[] catalogGuids = AssetDatabase.FindAssets("t:ItemCatalog");
            if (catalogGuids.Length == 0) issues.Add("No existe ningún ItemCatalog explícito.");
            foreach (string catalogGuid in catalogGuids)
            {
                string catalogPath = AssetDatabase.GUIDToAssetPath(catalogGuid);
                ItemCatalog catalog = AssetDatabase.LoadAssetAtPath<ItemCatalog>(catalogPath);
                foreach (InventoryItemData item in catalog.Items)
                {
                    if (item == null) { issues.Add($"Referencia vacía en {catalogPath}."); continue; }
                    if (string.IsNullOrWhiteSpace(item.ItemId)) issues.Add($"Item sin ID: {AssetDatabase.GetAssetPath(item)}");
                    else if (!itemIds.Add(item.ItemId)) issues.Add($"ItemId duplicado '{item.ItemId}' dentro de los catálogos.");
                }
            }

            var batteryIds = new HashSet<string>();
            if (GameFeatures.IsEnabled(OptionalGameFeature.Flashlight))
            {
                foreach (FlashlightController flashlight in Object.FindObjectsByType<FlashlightController>(FindObjectsInactive.Include))
                    CollectBatteryId(flashlight, batteryIds, issues);

                foreach (string prefabGuid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_EscapeRoomTemplate" }))
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(prefabGuid));
                    if (prefab == null) continue;
                    foreach (FlashlightController flashlight in prefab.GetComponentsInChildren<FlashlightController>(true))
                        CollectBatteryId(flashlight, batteryIds, issues);
                }
            }

            foreach (string batteryId in batteryIds)
                if (!itemIds.Contains(batteryId))
                    issues.Add($"La linterna requiere el ítem '{batteryId}', pero no existe en ningún ItemCatalog.");

            if (issues.Count == 0)
            {
                Debug.Log($"[Commercial Validator] OK — {documents} UIDocuments, {puzzles.Length} puzles, {saveIds.Count} estados persistentes y {itemIds.Count} ítems verificados.");
                return;
            }

            Debug.LogWarning($"[Commercial Validator] {issues.Count} incidencia(s):\n- {string.Join("\n- ", issues)}");
        }

        private static void CollectBatteryId(FlashlightController flashlight, HashSet<string> batteryIds, List<string> issues)
        {
            var serialized = new SerializedObject(flashlight);
            string batteryId = serialized.FindProperty("_batteryItemId")?.stringValue;
            if (string.IsNullOrWhiteSpace(batteryId))
            {
                issues.Add($"La linterna '{flashlight.name}' no tiene Battery Item ID.");
                return;
            }

            batteryIds.Add(batteryId);
        }
    }
}
#endif
