#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using EscapeRoomRevolt.Core.Flow;
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

            CheckObjectiveCycles(issues);
            CheckBrokenCombinationRecipes(issues);
            CheckMenuUxmlContract(issues);

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

        /// <summary>Depth-first cycle detection over every ObjectiveDefinition asset's Prerequisites graph. A cycle means every objective in it can never become available (IsAvailable requires every prerequisite already complete).</summary>
        private static void CheckObjectiveCycles(List<string> issues)
        {
            ObjectiveDefinition[] objectives = AssetDatabase.FindAssets("t:ObjectiveDefinition")
                .Select(guid => AssetDatabase.LoadAssetAtPath<ObjectiveDefinition>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(objective => objective != null)
                .ToArray();

            var state = new Dictionary<ObjectiveDefinition, int>(); // 0 unvisited, 1 in-progress, 2 done
            var reportedCycles = new HashSet<string>();

            foreach (ObjectiveDefinition objective in objectives)
                FindObjectiveCycle(objective, state, new List<ObjectiveDefinition>(), issues, reportedCycles);
        }

        private static void FindObjectiveCycle(ObjectiveDefinition current, Dictionary<ObjectiveDefinition, int> state,
            List<ObjectiveDefinition> path, List<string> issues, HashSet<string> reportedCycles)
        {
            if (state.TryGetValue(current, out int currentState))
            {
                if (currentState == 1)
                {
                    int startIndex = path.IndexOf(current);
                    string cycle = string.Join(" → ", path.Skip(startIndex).Select(o => o.ObjectiveId)) + " → " + current.ObjectiveId;
                    if (reportedCycles.Add(cycle)) issues.Add($"Ciclo de prerequisitos entre objetivos: {cycle}.");
                }
                return;
            }

            state[current] = 1;
            path.Add(current);
            foreach (ObjectiveDefinition prerequisite in current.Prerequisites)
                if (prerequisite != null) FindObjectiveCycle(prerequisite, state, path, issues, reportedCycles);
            path.RemoveAt(path.Count - 1);
            state[current] = 2;
        }

        /// <summary>Every InventoryItemData's Combinations list references CombineWith/ResultItem by direct object reference, so a deleted item asset leaves a silent null instead of a broken guid.</summary>
        private static void CheckBrokenCombinationRecipes(List<string> issues)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:InventoryItemData"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                InventoryItemData item = AssetDatabase.LoadAssetAtPath<InventoryItemData>(path);
                if (item == null || !item.CanCombine) continue;

                for (int i = 0; i < item.Combinations.Count; i++)
                {
                    ItemCombination combination = item.Combinations[i];
                    if (combination.CombineWith == null)
                        issues.Add($"Receta rota en '{path}': la combinación #{i} no tiene 'Combine With'.");
                    if (combination.ResultItem == null)
                        issues.Add($"Receta rota en '{path}': la combinación #{i} no tiene 'Result Item'.");
                }
            }
        }

        /// <summary>
        /// UIToolkitMenuController.OnEnable queries these element names by string
        /// (_root.Q&lt;Label&gt;("title") etc.) with no compile-time link to the UXML — renaming or
        /// deleting one of them in the UXML editor breaks the menu silently at runtime. Actually
        /// loads and clones EscapeRoomMenu.uxml rather than text-matching, so this catches the same
        /// failure UI Toolkit itself would hit.
        /// </summary>
        private static void CheckMenuUxmlContract(List<string> issues)
        {
            const string uxmlPath = "Assets/_EscapeRoomTemplate/UI/Toolkit/EscapeRoomMenu.uxml";
            VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            if (tree == null) { issues.Add($"No se encuentra '{uxmlPath}'."); return; }

            VisualElement root = tree.CloneTree();
            string[] requiredNames = { "title", "screen-content" };
            foreach (string elementName in requiredNames)
                if (root.Q<VisualElement>(elementName) == null)
                    issues.Add($"'{uxmlPath}' no contiene un elemento llamado '{elementName}', requerido por UIToolkitMenuController.");
        }
    }
}
#endif
