#if UNITY_EDITOR
using System;
using System.IO;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

namespace EscapeRoomRevolt.EditorTools
{
    public static class SurvivalNavigationTools
    {
        private const string ScenePath = "Assets/_EscapeRoomTemplate/Scenes/SurvivalHorrorDemo.unity";
        private const string NavigationFolder = "Assets/_EscapeRoomTemplate/Scenes/Navigation";

        [MenuItem("Escape Room Framework/Validation/Repair Survival Demo Navigation", priority = 703)]
        public static void RepairDemoNavigation()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var setup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                var surfaces = UnityEngine.Object.FindObjectsByType<NavMeshSurface>(FindObjectsSortMode.None);
                if (surfaces.Length == 0) throw new InvalidOperationException("Survival demo has no navigation surface.");
                foreach (var surface in surfaces)
                {
                    if (surface.navMeshData != null && EditorUtility.IsPersistent(surface.navMeshData)) continue;
                    // Bake static environment only; live pickups/characters must not become obstacles in the asset.
                    surface.collectObjects = CollectObjects.Children;
                    surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
                    surface.BuildNavMesh();
                    Persist(surface);
                    EditorUtility.SetDirty(surface);
                }
                if (NavMesh.CalculateTriangulation().vertices.Length == 0) throw new InvalidOperationException("Survival navigation bake produced no walkable geometry.");
                foreach (var agent in UnityEngine.Object.FindObjectsByType<NavMeshAgent>(FindObjectsSortMode.None))
                    if (!NavMesh.SamplePosition(agent.transform.position, out _, 2f, new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = NavMesh.AllAreas }))
                        throw new InvalidOperationException("Enemy is outside its baked navigation: " + agent.name);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                AssetDatabase.SaveAssets();
                Debug.Log("[Survival Navigation] PASS: persistent navigation asset and sampled enemy spawn positions.");
            }
            finally
            {
                if (setup.Length > 0) EditorSceneManager.RestoreSceneManagerSetup(setup);
            }
        }

        public static void Persist(NavMeshSurface surface)
        {
            if (surface.navMeshData == null) throw new InvalidOperationException("No navigation data to persist.");
            if (!AssetDatabase.IsValidFolder(NavigationFolder))
            {
                Directory.CreateDirectory(NavigationFolder);
                AssetDatabase.Refresh();
            }
            string path = NavigationFolder + "/SurvivalHorrorDemo_NavMesh.asset";
            var existing = AssetDatabase.LoadAssetAtPath<NavMeshData>(path);
            if (existing == null) AssetDatabase.CreateAsset(surface.navMeshData, path);
            else if (existing != surface.navMeshData)
            {
                var generated = surface.navMeshData;
                surface.RemoveData();
                EditorUtility.CopySerialized(generated, existing);
                surface.navMeshData = existing;
                surface.AddData();
                EditorUtility.SetDirty(existing);
                UnityEngine.Object.DestroyImmediate(generated);
            }
        }
    }
}
#endif
