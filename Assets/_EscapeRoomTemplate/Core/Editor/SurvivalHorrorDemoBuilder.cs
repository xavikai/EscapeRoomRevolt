#if UNITY_EDITOR
using System.Collections.Generic;
using EscapeRoomRevolt.Core.Flow;
using EscapeRoomRevolt.Core.Settings;
using EscapeRoomRevolt.Systems.Interaction;
using EscapeRoomRevolt.Systems.Inventory;
using EscapeRoomRevolt.Systems.Survival;
using EscapeRoomRevolt.Systems.Equipment;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace EscapeRoomRevolt.Core.Editor
{
    /// <summary>Creates a replaceable-art survival vertical slice entirely from project-owned primitives.</summary>
    public static class SurvivalHorrorDemoBuilder
    {
        private const string Root = "Assets/_EscapeRoomTemplate";
        private const string ScenePath = Root + "/Scenes/SurvivalHorrorDemo.unity";
        private const string PrefabFolder = Root + "/Prefabs/Survival";
        private const string ProfileFolder = Root + "/ScriptableObjects/Survival";
        private const string EnemyPrefabPath = PrefabFolder + "/HorrorEnemy_Modular.prefab";
        private const string HidingPrefabPath = PrefabFolder + "/HidingLocker_Modular.prefab";
        private const string HidingBedPrefabPath = PrefabFolder + "/HidingBed_Modular.prefab";
        private const string CamcorderPrefabPath = PrefabFolder + "/Camcorder_Modular.prefab";
        private const string EnemyProfilePath = ProfileFolder + "/DemoStalkerProfile.asset";
        private const string ObjectiveSetPath = ProfileFolder + "/DemoObjectiveSet.asset";
        private const string DifficultySettingsPath = Root + "/Resources/SurvivalDifficultySettings.asset";
        private const string CamcorderBatteryPath = Root + "/ScriptableObjects/Items/CamcorderBattery.asset";
        private const string EvidencePath = ProfileFolder + "/Evidence_AnomalySubject.asset";

        [MenuItem("Escape Room Framework/Demo/Create or Update Survival Horror Demo", priority = 50)]
        public static void Build()
        {
            EnsureFolder(PrefabFolder);
            EnsureFolder(ProfileFolder);
            EnsureDifficultyPresets();
            InventoryItemData camcorderBattery = EnsureCamcorderBattery();
            EnsureCatalogContains(camcorderBattery);
            EvidenceDefinition evidence = EnsureEvidence();
            HorrorEnemyProfile profile = EnsureEnemyProfile();
            ObjectiveSet objectiveSet = EnsureObjectives();
            GameObject enemyPrefab = BuildEnemyPrefab(profile);
            GameObject hidingPrefab = BuildHidingPrefab();
            GameObject hidingBedPrefab = BuildHidingBedPrefab();
            GameObject camcorderPrefab = BuildCamcorderPrefab();
            BuildScene(enemyPrefab, hidingPrefab, hidingBedPrefab, camcorderPrefab,
                profile, objectiveSet, camcorderBattery, evidence);
            UseSurvivalProfileAndDemoStart();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Survival Demo] Scene, modular prefabs and Survival Horror profile are ready.");
        }

        private static void EnsureDifficultyPresets()
        {
            SurvivalDifficultyProfile easy = EnsureAsset<SurvivalDifficultyProfile>(ProfileFolder + "/Difficulty_Easy.asset");
            easy.Configure("easy", "Accessible", .75f, .8f, 1.25f, 1f, .9f, .85f, .85f, .85f, 1.15f, 1.25f, .75f, true, true);
            SurvivalDifficultyProfile standard = EnsureAsset<SurvivalDifficultyProfile>(ProfileFolder + "/Difficulty_Standard.asset");
            standard.Configure("standard", "Standard", 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, true, true);
            SurvivalDifficultyProfile nightmare = EnsureAsset<SurvivalDifficultyProfile>(ProfileFolder + "/Difficulty_Nightmare.asset");
            nightmare.Configure("nightmare", "Nightmare", 1.2f, 1.25f, .75f, .5f, 1.15f, 1.2f, 1.25f, 1.15f, .75f, .65f, 1.4f, false, false);
            EditorUtility.SetDirty(easy);
            EditorUtility.SetDirty(standard);
            EditorUtility.SetDirty(nightmare);

            SurvivalDifficultySettings settings = EnsureAsset<SurvivalDifficultySettings>(DifficultySettingsPath);
            settings.Configure(new[] { easy, standard, nightmare }, standard);
            EditorUtility.SetDirty(settings);
        }

        private static HorrorEnemyProfile EnsureEnemyProfile()
        {
            HorrorEnemyProfile profile = AssetDatabase.LoadAssetAtPath<HorrorEnemyProfile>(EnemyProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<HorrorEnemyProfile>();
                AssetDatabase.CreateAsset(profile, EnemyProfilePath);
            }
            profile.detectionSeconds = .65f;
            profile.awarenessDecayPerSecond = .55f;
            profile.instantDetectionRange = 1.75f;
            profile.useVisibilityModifiers = true;
            profile.inspectHidingSpots = true;
            profile.hidingInspectionDelay = 1.35f;
            profile.operateDoors = true;
            profile.forceLockedDoors = false;
            profile.slamDoorsDuringChase = true;
            profile.doorInteractionDistance = 1.6f;
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static ObjectiveSet EnsureObjectives()
        {
            EndingDefinition ending = EnsureAsset<EndingDefinition>(ProfileFolder + "/DemoEscapeEnding.asset");
            SetString(ending, "_endingId", "survival_demo_escape");
            SetString(ending, "_title", "Has escapat de la instal·lació");
            SetString(ending, "_message", "La sortida és oberta, però la investigació només acaba de començar.");

            ObjectiveDefinition battery = EnsureObjective(
                ProfileFolder + "/Objective_RecoverBattery.asset",
                "recover_batteries", "Recupera piles de recanvi",
                "Busca un recurs d'energia abans d'endinsar-te a la instal·lació.",
                ObjectiveTrigger.ItemCollected, "batteries", null);
            ObjectiveDefinition power = EnsureObjective(
                ProfileFolder + "/Objective_RestorePower.asset",
                "restore_power", "Restableix l'energia d'emergència",
                "Localitza el generador de la ruta lateral i activa'l sense alertar la presència.",
                ObjectiveTrigger.InteractionPerformed, "EmergencyGenerator", new[] { battery });
            ObjectiveDefinition evidence = EnsureObjective(
                ProfileFolder + "/Objective_RecordEvidence.asset",
                "record_anomaly", "Grava l'anomalia",
                "Equipa la càmera i mantén el subjecte enquadrat fins que l'evidència quedi arxivada.",
                ObjectiveTrigger.EvidenceRecorded, "anomaly_subject", new[] { power });
            ObjectiveDefinition escape = EnsureObjective(
                ProfileFolder + "/Objective_EscapeFacility.asset",
                "escape_facility", "Arriba a la sortida",
                "Travessa la zona de patrulla i arriba al final del corredor.",
                ObjectiveTrigger.Manual, string.Empty, new[] { evidence });

            ObjectiveSet set = EnsureAsset<ObjectiveSet>(ObjectiveSetPath);
            SetString(set, "_roomId", "survival_horror_demo");
            SetObjectArray(set, "_objectives", new Object[] { battery, power, evidence, escape });
            SetReference(set, "_completionEnding", ending);
            return set;
        }

        private static InventoryItemData EnsureCamcorderBattery()
        {
            InventoryItemData item = EnsureAsset<InventoryItemData>(CamcorderBatteryPath);
            SetString(item, "_itemId", "camcorder_battery");
            SetString(item, "_displayName", "Bateria de càmera");
            SetString(item, "_description", "Bateria específica per a la visió nocturna de la càmera.");
            SetEnum(item, "_category", (int)InventoryItemCategory.Consumable);
            SetEnum(item, "_primaryAction", (int)InventoryPrimaryAction.None);
            SetBool(item, "_isConsumable", true);
            SetBool(item, "_isStackable", true);
            SetInt(item, "_maxStack", 8);
            SetBool(item, "_canDrop", false);
            SetBool(item, "_canExamine", false);
            return item;
        }

        private static EvidenceDefinition EnsureEvidence()
        {
            EvidenceDefinition evidence = EnsureAsset<EvidenceDefinition>(EvidencePath);
            SetString(evidence, "_evidenceId", "anomaly_subject");
            SetString(evidence, "_title", "Subjecte anòmal");
            SetString(evidence, "_description", "Una figura immòbil que no apareix als registres de la instal·lació.");
            SetFloat(evidence, "_recordingSeconds", 2.5f);
            SetFloat(evidence, "_maximumDistance", 14f);
            return evidence;
        }

        private static void EnsureCatalogContains(InventoryItemData item)
        {
            ItemCatalog catalog = AssetDatabase.LoadAssetAtPath<ItemCatalog>(Root + "/ScriptableObjects/DefaultItemCatalog.asset");
            if (catalog == null || item == null) return;
            var items = new List<Object>();
            foreach (InventoryItemData existing in catalog.Items)
                if (existing != null && !items.Contains(existing)) items.Add(existing);
            if (!items.Contains(item)) items.Add(item);
            SetObjectArray(catalog, "_items", items.ToArray());
        }

        private static ObjectiveDefinition EnsureObjective(string path, string id, string title, string description,
            ObjectiveTrigger trigger, string targetId, ObjectiveDefinition[] prerequisites)
        {
            ObjectiveDefinition objective = EnsureAsset<ObjectiveDefinition>(path);
            SetString(objective, "_objectiveId", id);
            SetString(objective, "_title", title);
            SetString(objective, "_description", description);
            SetEnum(objective, "_trigger", (int)trigger);
            SetString(objective, "_targetId", targetId);
            SetObjectArray(objective, "_prerequisites", prerequisites ?? System.Array.Empty<ObjectiveDefinition>());
            SetBool(objective, "_hiddenUntilAvailable", prerequisites != null && prerequisites.Length > 0);
            return objective;
        }

        private static T EnsureAsset<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static GameObject BuildEnemyPrefab(HorrorEnemyProfile profile)
        {
            GameObject root = new GameObject("HorrorEnemy_Modular");
            root.AddComponent<NavMeshAgent>();
            HorrorEnemyController enemy = root.AddComponent<HorrorEnemyController>();

            Transform eye = new GameObject("Eye").transform;
            eye.SetParent(root.transform, false);
            eye.localPosition = new Vector3(0f, 1.65f, .12f);

            Transform socket = new GameObject("ModelSocket").transform;
            socket.SetParent(root.transform, false);
            GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            placeholder.name = "Placeholder_ReplaceMe";
            placeholder.transform.SetParent(socket, false);
            placeholder.transform.localPosition = Vector3.up;
            Object.DestroyImmediate(placeholder.GetComponent<Collider>());

            ReplaceableModelSlot slot = root.AddComponent<ReplaceableModelSlot>();
            SetReference(slot, "_modelSocket", socket);
            SetReference(slot, "_placeholderVisual", placeholder);
            enemy.Configure(profile, eye, null);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, EnemyPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject BuildHidingPrefab()
        {
            GameObject root = new GameObject("HidingLocker_Modular");
            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 1.1f, 0f);
            collider.size = new Vector3(1.5f, 2.2f, .9f);
            NavMeshObstacle obstacle = root.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.size = collider.size;
            obstacle.center = collider.center;
            obstacle.carving = true;
            HidingSpot hiding = root.AddComponent<HidingSpot>();

            Transform inside = new GameObject("InsideAnchor").transform;
            inside.SetParent(root.transform, false);
            inside.localPosition = new Vector3(0f, 0f, .1f);
            inside.localRotation = Quaternion.Euler(0f, 180f, 0f);
            Transform exit = new GameObject("ExitAnchor").transform;
            exit.SetParent(root.transform, false);
            exit.localPosition = new Vector3(0f, 0f, -1.45f);
            Transform inspection = new GameObject("InspectionAnchor").transform;
            inspection.SetParent(root.transform, false);
            inspection.localPosition = new Vector3(0f, 0f, -1.05f);

            Transform socket = new GameObject("ModelSocket").transform;
            socket.SetParent(root.transform, false);
            GameObject placeholder = new GameObject("Placeholder_ReplaceMe");
            placeholder.transform.SetParent(socket, false);
            CreateChildCube(placeholder.transform, "Back", new Vector3(0f, 1.1f, .42f), new Vector3(1.5f, 2.2f, .08f));
            CreateChildCube(placeholder.transform, "Left", new Vector3(-.71f, 1.1f, 0f), new Vector3(.08f, 2.2f, .9f));
            CreateChildCube(placeholder.transform, "Right", new Vector3(.71f, 1.1f, 0f), new Vector3(.08f, 2.2f, .9f));
            CreateChildCube(placeholder.transform, "Top", new Vector3(0f, 2.16f, 0f), new Vector3(1.5f, .08f, .9f));

            ReplaceableModelSlot slot = root.AddComponent<ReplaceableModelSlot>();
            SetReference(slot, "_modelSocket", socket);
            SetReference(slot, "_placeholderVisual", placeholder);
            SetReference(hiding, "_insideAnchor", inside);
            SetReference(hiding, "_exitAnchor", exit);
            SetReference(hiding, "_inspectionAnchor", inspection);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, HidingPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject BuildHidingBedPrefab()
        {
            GameObject root = new GameObject("HidingBed_Modular");
            BoxCollider topCollider = root.AddComponent<BoxCollider>();
            topCollider.center = new Vector3(0f, .92f, 0f);
            topCollider.size = new Vector3(2.7f, .18f, 2.1f);
            NavMeshObstacle obstacle = root.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.size = new Vector3(2.7f, 1f, 2.1f);
            obstacle.center = new Vector3(0f, .5f, 0f);
            obstacle.carving = true;
            HidingSpot hiding = root.AddComponent<HidingSpot>();

            Transform inside = new GameObject("InsideAnchor").transform;
            inside.SetParent(root.transform, false);
            inside.localPosition = Vector3.zero;
            inside.localRotation = Quaternion.Euler(0f, 180f, 0f);
            Transform exit = new GameObject("ExitAnchor").transform;
            exit.SetParent(root.transform, false);
            exit.localPosition = new Vector3(0f, 0f, -1.55f);
            Transform inspection = new GameObject("InspectionAnchor").transform;
            inspection.SetParent(root.transform, false);
            inspection.localPosition = new Vector3(0f, 0f, -1.25f);

            Transform socket = new GameObject("ModelSocket").transform;
            socket.SetParent(root.transform, false);
            GameObject placeholder = new GameObject("Placeholder_ReplaceMe");
            placeholder.transform.SetParent(socket, false);
            CreateChildCube(placeholder.transform, "Mattress", new Vector3(0f, .92f, 0f), new Vector3(2.7f, .18f, 2.1f));
            CreateChildCube(placeholder.transform, "Leg_FL", new Vector3(-1.2f, .45f, -.8f), new Vector3(.18f, .9f, .18f));
            CreateChildCube(placeholder.transform, "Leg_FR", new Vector3(1.2f, .45f, -.8f), new Vector3(.18f, .9f, .18f));
            CreateChildCube(placeholder.transform, "Leg_BL", new Vector3(-1.2f, .45f, .8f), new Vector3(.18f, .9f, .18f));
            CreateChildCube(placeholder.transform, "Leg_BR", new Vector3(1.2f, .45f, .8f), new Vector3(.18f, .9f, .18f));

            ReplaceableModelSlot slot = root.AddComponent<ReplaceableModelSlot>();
            SetReference(slot, "_modelSocket", socket);
            SetReference(slot, "_placeholderVisual", placeholder);
            SetReference(hiding, "_insideAnchor", inside);
            SetReference(hiding, "_exitAnchor", exit);
            SetReference(hiding, "_inspectionAnchor", inspection);
            SetEnum(hiding, "_kind", (int)HidingSpotKind.UnderBed);
            SetBool(hiding, "_forceCrouchedPose", true);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, HidingBedPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject BuildCamcorderPrefab()
        {
            GameObject root = new GameObject("Camcorder_Modular");
            Rigidbody body = root.AddComponent<Rigidbody>();
            body.mass = .8f;
            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 0f, .08f);
            collider.size = new Vector3(.42f, .28f, .62f);
            EquippableItem equippable = root.AddComponent<EquippableItem>();
            NightVisionController camcorder = root.AddComponent<NightVisionController>();
            root.AddComponent<CamcorderEvidenceRecorder>();

            Transform socket = new GameObject("ModelSocket").transform;
            socket.SetParent(root.transform, false);
            GameObject placeholder = new GameObject("Placeholder_ReplaceMe");
            placeholder.transform.SetParent(socket, false);
            CreateChildCube(placeholder.transform, "Body", Vector3.zero, new Vector3(.42f, .28f, .58f));
            CreateChildCube(placeholder.transform, "Screen", new Vector3(-.26f, .02f, .02f), new Vector3(.08f, .22f, .34f));
            GameObject lens = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lens.name = "Lens";
            lens.transform.SetParent(placeholder.transform, false);
            lens.transform.localPosition = new Vector3(0f, 0f, .36f);
            lens.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            lens.transform.localScale = new Vector3(.12f, .1f, .12f);
            Object.DestroyImmediate(lens.GetComponent<Collider>());

            GameObject illuminatorObject = new GameObject("NightVisionIlluminator");
            illuminatorObject.transform.SetParent(socket, false);
            illuminatorObject.transform.localPosition = new Vector3(0f, .08f, .34f);
            Light illuminator = illuminatorObject.AddComponent<Light>();
            illuminator.type = LightType.Spot;
            illuminator.intensity = .75f;
            illuminator.range = 16f;
            illuminator.spotAngle = 68f;
            illuminator.shadows = LightShadows.None;
            illuminator.enabled = false;

            ReplaceableModelSlot slot = root.AddComponent<ReplaceableModelSlot>();
            SetReference(slot, "_modelSocket", socket);
            SetReference(slot, "_placeholderVisual", placeholder);
            SetReference(camcorder, "_nightVisionIlluminator", illuminator);
            SetReference(camcorder, "_visualRoot", socket.gameObject);
            SetString(camcorder, "_batteryItemId", "camcorder_battery");
            SetString(equippable, "_equipmentId", "camcorder");
            SetString(equippable, "_interactionPrompt", "Equipar càmera");
            SetString(equippable, "_saveId", "CamcorderEquipment");
            SetVector3(equippable, "_equippedLocalPosition", new Vector3(.24f, -.18f, .48f));
            SetVector3(equippable, "_equippedLocalEulerAngles", new Vector3(2f, 0f, 0f));
            SetVector3(equippable, "_equippedLocalScale", Vector3.one);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, CamcorderPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void BuildScene(GameObject enemyPrefab, GameObject hidingPrefab, GameObject hidingBedPrefab,
            GameObject camcorderPrefab, HorrorEnemyProfile profile, ObjectiveSet objectiveSet,
            InventoryItemData camcorderBattery, EvidenceDefinition evidence)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "SurvivalHorrorDemo";

            GameObject managerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/GameManager.prefab");
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/Player_PC.prefab");
            if (managerPrefab != null) PrefabUtility.InstantiatePrefab(managerPrefab, scene);
            GameObject player = playerPrefab != null ? (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene) : null;
            if (player != null)
            {
                player.name = "Player_PC";
                player.transform.SetPositionAndRotation(new Vector3(0f, .05f, -16f), Quaternion.identity);
                player.tag = "Player";
            }

            GameObject geometry = new GameObject("ENVIRONMENT_Primitives_Replaceable");
            CreateCube(geometry.transform, "Floor", new Vector3(0f, -.1f, 0f), new Vector3(11f, .2f, 38f));
            CreateCube(geometry.transform, "Ceiling", new Vector3(0f, 3.5f, 0f), new Vector3(11f, .2f, 38f));
            CreateCube(geometry.transform, "Wall_Left", new Vector3(-5.4f, 1.7f, 0f), new Vector3(.2f, 3.4f, 38f));
            CreateCube(geometry.transform, "Wall_Right_A", new Vector3(5.4f, 1.7f, -11f), new Vector3(.2f, 3.4f, 16f));
            CreateCube(geometry.transform, "Wall_Right_B1", new Vector3(5.4f, 1.7f, 5f), new Vector3(.2f, 3.4f, 4f));
            CreateCube(geometry.transform, "Wall_Right_B2", new Vector3(5.4f, 1.7f, 15f), new Vector3(.2f, 3.4f, 8f));
            CreateCube(geometry.transform, "Wall_Start", new Vector3(0f, 1.7f, -18.9f), new Vector3(11f, 3.4f, .2f));
            CreateCube(geometry.transform, "Wall_End", new Vector3(0f, 1.7f, 18.9f), new Vector3(11f, 3.4f, .2f));
            CreateCube(geometry.transform, "SideRouteFloor", new Vector3(8f, -.1f, 4.5f), new Vector3(5.2f, .2f, 15f));
            CreateCube(geometry.transform, "SideRouteBack", new Vector3(10.5f, 1.7f, 4.5f), new Vector3(.2f, 3.4f, 15f));
            CreateCube(geometry.transform, "SideRouteEnd", new Vector3(8f, 1.7f, 11.9f), new Vector3(5.2f, 3.4f, .2f));
            CreateCube(geometry.transform, "SideRoomSouth", new Vector3(8f, 1.7f, -2.9f), new Vector3(5.2f, 3.4f, .2f));
            CreateCube(geometry.transform, "ChaseDoorWall_Left", new Vector3(-3.05f, 1.7f, -5f), new Vector3(4.7f, 3.4f, .2f));
            CreateCube(geometry.transform, "ChaseDoorWall_Right", new Vector3(3.05f, 1.7f, -5f), new Vector3(4.7f, 3.4f, .2f));
            CreateCube(geometry.transform, "LadderPlatform_Replaceable", new Vector3(-3.7f, .6f, .6f), new Vector3(2.2f, 1.2f, 2.8f));

            NavMeshSurface surface = geometry.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.All;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.BuildNavMesh();

            GameObject aiRoot = new GameObject("AI");
            Transform[] patrol = new Transform[4];
            Vector3[] points = { new Vector3(0f, 0f, 12f), new Vector3(0f, 0f, 16f), new Vector3(8f, 0f, 9f), new Vector3(0f, 0f, 5f) };
            for (int i = 0; i < patrol.Length; i++)
            {
                patrol[i] = new GameObject($"Patrol_{i + 1:00}").transform;
                patrol[i].SetParent(aiRoot.transform);
                patrol[i].position = points[i];
            }

            GameObject enemy = (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab, scene);
            enemy.transform.position = points[0];
            enemy.GetComponent<HorrorEnemyController>().Configure(profile, enemy.transform.Find("Eye"), patrol);

            GameObject locker = (GameObject)PrefabUtility.InstantiatePrefab(hidingPrefab, scene);
            locker.name = "HidingLocker_Modular";
            locker.transform.SetPositionAndRotation(new Vector3(9.2f, 0f, 4f), Quaternion.Euler(0f, -90f, 0f));
            GameObject bed = (GameObject)PrefabUtility.InstantiatePrefab(hidingBedPrefab, scene);
            bed.name = "HidingBed_Modular";
            bed.transform.SetPositionAndRotation(new Vector3(7.5f, 0f, 8.7f), Quaternion.identity);
            if (camcorderPrefab != null)
            {
                GameObject camcorder = (GameObject)PrefabUtility.InstantiatePrefab(camcorderPrefab, scene);
                camcorder.name = "Camcorder_Modular";
                camcorder.transform.SetPositionAndRotation(new Vector3(1.5f, .45f, -14.2f), Quaternion.identity);
            }

            Door securityDoor = CreateSecurityDoor(new Vector3(0f, 1.5f, 7.5f));
            CreateAIDoor(new Vector3(0f, 1.3f, -5f));
            CreatePowerConsole(new Vector3(9.2f, .65f, .5f), securityDoor);
            GameObject objectives = new GameObject("SurvivalObjectives");
            ObjectiveManager objectiveManager = objectives.AddComponent<ObjectiveManager>();
            SetReference(objectiveManager, "_objectiveSet", objectiveSet);

            CreateCheckpoint("Checkpoint_Start", new Vector3(0f, .5f, -15f), true);
            CreateCheckpoint("Checkpoint_Mid", new Vector3(0f, .5f, -2f), false);
            CreateBatteryPickup(new Vector3(-2.5f, .45f, -8f));
            CreateInventoryPickup("CamcorderBatteryPickup_Demo", camcorderBattery, 2, new Vector3(-1.5f, .3f, -11f));
            CreateThrowableProp("ThrowableCan_01", new Vector3(-1.7f, .45f, -10f));
            CreateThrowableProp("ThrowableCan_02", new Vector3(2.1f, .45f, -8.6f));
            CreateVisibilityZone(new Vector3(8f, 1.6f, 4.5f), new Vector3(5f, 3.2f, 14f), .35f);
            CreateDamageHazard(new Vector3(-3.7f, .04f, 2f));
            CreateTraversalObstacle("Traversal_Vault_Demo", TraversalType.Vault,
                new Vector3(0f, .45f, -12f), new Vector3(2.8f, .9f, .35f), .8f, .85f,
                "Saltar obstacle", EnemyTraversalPolicy.UseTraversal);
            CreateTraversalObstacle("Traversal_Climb_Demo", TraversalType.Climb,
                new Vector3(3.6f, .75f, -1f), new Vector3(2.2f, 1.5f, .4f), 1.05f, 1.15f,
                "Escalar obstacle", EnemyTraversalPolicy.RouteAround);
            CreateTraversalObstacle("Traversal_Ladder_Demo", TraversalType.Ladder,
                new Vector3(-3.7f, .6f, -1.1f), new Vector3(.8f, 1.2f, .18f), 1.25f, 0f,
                "Pujar escala", EnemyTraversalPolicy.Blocked,
                new Vector3(0f, -.5f, -3f), new Vector3(0f, .5f, 3f));
            CreateSqueezeObstacle(new Vector3(0f, 0f, 13f));
            CreateEvidenceSubject(evidence, new Vector3(-2.9f, 1.05f, 10.4f));
            new GameObject("GameplayNoise_Debug").AddComponent<GameplayNoiseDebugVisualizer>();
            CreateExit(new Vector3(0f, 1f, 17.5f));
            CreateLighting();
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);
        }

        private static void CreateCheckpoint(string name, Vector3 position, bool initial)
        {
            GameObject checkpoint = new GameObject(name);
            checkpoint.transform.position = position;
            BoxCollider trigger = checkpoint.AddComponent<BoxCollider>();
            trigger.size = new Vector3(8f, 2f, 1f);
            trigger.isTrigger = true;
            SurvivalCheckpoint component = checkpoint.AddComponent<SurvivalCheckpoint>();
            SetString(component, "_checkpointId", name);
            SetBool(component, "_isInitial", initial);
        }

        private static void CreateBatteryPickup(Vector3 position)
        {
            InventoryItemData batteries = AssetDatabase.LoadAssetAtPath<InventoryItemData>(Root + "/ScriptableObjects/Items/Batteries.asset");
            if (batteries == null) return;
            GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pickup.name = "BatteryPickup_Demo";
            pickup.transform.position = position;
            pickup.transform.localScale = new Vector3(.25f, .08f, .25f);
            PickableItem item = pickup.AddComponent<PickableItem>();
            pickup.AddComponent<CheckpointEntity>();
            SetReference(item, "_itemData", batteries);
            SetInt(item, "_quantity", 2);
        }

        private static void CreateInventoryPickup(string name, InventoryItemData data, int quantity, Vector3 position)
        {
            if (data == null) return;
            GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pickup.name = name;
            pickup.transform.position = position;
            pickup.transform.localScale = new Vector3(.2f, .08f, .2f);
            PickableItem item = pickup.AddComponent<PickableItem>();
            pickup.AddComponent<CheckpointEntity>();
            SetReference(item, "_itemData", data);
            SetInt(item, "_quantity", quantity);
        }

        private static void CreateEvidenceSubject(EvidenceDefinition definition, Vector3 position)
        {
            GameObject root = new GameObject("Evidence_AnomalySubject_Modular");
            root.transform.position = position;
            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 0f, 0f);
            collider.size = new Vector3(.9f, 2.1f, .55f);
            RecordableEvidence recordable = root.AddComponent<RecordableEvidence>();
            recordable.Configure(definition);

            Transform socket = new GameObject("ModelSocket").transform;
            socket.SetParent(root.transform, false);
            GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            placeholder.name = "Placeholder_ReplaceMe";
            placeholder.transform.SetParent(socket, false);
            Object.DestroyImmediate(placeholder.GetComponent<Collider>());
            ReplaceableModelSlot slot = root.AddComponent<ReplaceableModelSlot>();
            SetReference(slot, "_modelSocket", socket);
            SetReference(slot, "_placeholderVisual", placeholder);
        }

        private static void CreateExit(Vector3 position)
        {
            GameObject exit = new GameObject("DemoExit_Victory");
            exit.transform.position = position;
            BoxCollider trigger = exit.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(8f, 2f, 1f);
            SurvivalObjectiveZone ending = exit.AddComponent<SurvivalObjectiveZone>();
            SetString(ending, "_objectiveId", "escape_facility");
            exit.AddComponent<ChaseSafeZone>();
        }

        private static void CreateThrowableProp(string name, Vector3 position)
        {
            GameObject prop = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            prop.name = name;
            prop.transform.position = position;
            prop.transform.localScale = new Vector3(.18f, .38f, .18f);
            Rigidbody body = prop.AddComponent<Rigidbody>();
            body.mass = .65f;
            prop.AddComponent<PhysicsGrabbable>();
        }

        private static void CreateDamageHazard(Vector3 position)
        {
            GameObject hazard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hazard.name = "DamageHazard_Demo";
            hazard.transform.position = position;
            hazard.transform.localScale = new Vector3(2f, .08f, 2f);
            DamageVolume damage = hazard.AddComponent<DamageVolume>();
            SetFloat(damage, "_damage", 35f);
            SetEnum(damage, "_damageType", (int)DamageType.Trap);
            SetFloat(damage, "_repeatInterval", 1.2f);
        }

        private static TraversalObstacle CreateTraversalObstacle(string name, TraversalType type, Vector3 position,
            Vector3 scale, float duration, float arcHeight, string prompt, EnemyTraversalPolicy enemyPolicy,
            Vector3? entryLocalPosition = null, Vector3? exitLocalPosition = null)
        {
            GameObject obstacleObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstacleObject.name = name;
            obstacleObject.transform.position = position;
            obstacleObject.transform.localScale = scale;
            TraversalObstacle obstacle = obstacleObject.AddComponent<TraversalObstacle>();

            Transform entry = new GameObject("EntryAnchor").transform;
            entry.SetParent(obstacleObject.transform, false);
            entry.localPosition = entryLocalPosition ?? new Vector3(0f, -.5f, -3f);
            Transform exit = new GameObject("ExitAnchor").transform;
            exit.SetParent(obstacleObject.transform, false);
            exit.localPosition = exitLocalPosition ?? new Vector3(0f, -.5f, 3f);
            SetReference(obstacle, "_entryAnchor", entry);
            SetReference(obstacle, "_exitAnchor", exit);
            SetEnum(obstacle, "_type", (int)type);
            SetFloat(obstacle, "_duration", duration);
            SetFloat(obstacle, "_arcHeight", arcHeight);
            SetString(obstacle, "_prompt", prompt);
            SetEnum(obstacle, "_enemyPolicy", (int)enemyPolicy);

            NavMeshObstacle navObstacle = obstacleObject.AddComponent<NavMeshObstacle>();
            navObstacle.shape = NavMeshObstacleShape.Box;
            navObstacle.size = Vector3.one;
            navObstacle.carving = true;
            navObstacle.enabled = enemyPolicy != EnemyTraversalPolicy.UseTraversal;
            return obstacle;
        }

        private static void CreateSqueezeObstacle(Vector3 position)
        {
            GameObject root = new GameObject("Traversal_Squeeze_Demo");
            root.transform.position = position;
            BoxCollider interactionBlocker = root.AddComponent<BoxCollider>();
            interactionBlocker.center = new Vector3(0f, 1.25f, 0f);
            interactionBlocker.size = new Vector3(1f, 2.5f, .35f);
            TraversalObstacle obstacle = root.AddComponent<TraversalObstacle>();
            CreateChildCube(root.transform, "SqueezeWall_Left_Replaceable",
                new Vector3(-2.8f, 1.7f, 0f), new Vector3(4.6f, 3.4f, .35f));
            CreateChildCube(root.transform, "SqueezeWall_Right_Replaceable",
                new Vector3(2.8f, 1.7f, 0f), new Vector3(4.6f, 3.4f, .35f));

            Transform entry = new GameObject("EntryAnchor").transform;
            entry.SetParent(root.transform, false);
            entry.localPosition = new Vector3(0f, 0f, -1.15f);
            Transform exit = new GameObject("ExitAnchor").transform;
            exit.SetParent(root.transform, false);
            exit.localPosition = new Vector3(0f, 0f, 1.15f);
            SetReference(obstacle, "_entryAnchor", entry);
            SetReference(obstacle, "_exitAnchor", exit);
            SetEnum(obstacle, "_type", (int)TraversalType.Squeeze);
            SetEnum(obstacle, "_enemyPolicy", (int)EnemyTraversalPolicy.UseTraversal);
            SetFloat(obstacle, "_duration", 1.15f);
            SetFloat(obstacle, "_arcHeight", 0f);
            SetString(obstacle, "_prompt", "Passar de costat");

            NavMeshObstacle navObstacle = root.AddComponent<NavMeshObstacle>();
            navObstacle.shape = NavMeshObstacleShape.Box;
            navObstacle.size = new Vector3(1f, 2.5f, .35f);
            navObstacle.center = new Vector3(0f, 1.25f, 0f);
            navObstacle.carving = true;
            navObstacle.enabled = false;
        }

        private static void CreateVisibilityZone(Vector3 position, Vector3 size, float multiplier)
        {
            GameObject zone = new GameObject("DarkRoute_VisibilityZone");
            zone.transform.position = position;
            BoxCollider trigger = zone.AddComponent<BoxCollider>();
            trigger.size = size;
            trigger.isTrigger = true;
            VisibilityZone visibility = zone.AddComponent<VisibilityZone>();
            SetFloat(visibility, "_visibilityMultiplier", multiplier);
        }

        private static Door CreateAIDoor(Vector3 position)
        {
            GameObject doorObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorObject.name = "ChaseDoor_AICompatible";
            doorObject.transform.position = position;
            doorObject.transform.localScale = new Vector3(1.4f, 2.6f, .18f);
            Door door = doorObject.AddComponent<Door>();
            SetBool(door, "_allowEnemyOperation", true);
            SetBool(door, "_enemyCanBreakLock", false);
            SetBool(door, "_enableAdvancedOperations", true);
            SetFloat(door, "_peekFraction", .18f);
            SetFloat(door, "_carefulDuration", 2.4f);
            SetFloat(door, "_slamDuration", .18f);
            SetFloat(door, "_openAngle", 105f);
            SetString(door, "_openPrompt", "Obrir porta");
            Transform hinge = new GameObject("Hinge").transform;
            hinge.SetParent(doorObject.transform, false);
            hinge.localPosition = new Vector3(-.5f, 0f, 0f);
            SetReference(door, "_customPivot", hinge);
            NavMeshObstacle obstacle = doorObject.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.size = Vector3.one;
            obstacle.carving = true;
            return door;
        }

        private static Door CreateSecurityDoor(Vector3 position)
        {
            GameObject gate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gate.name = "SecurityGate";
            gate.transform.position = position;
            gate.transform.localScale = new Vector3(10.2f, 3f, .25f);
            Door door = gate.AddComponent<Door>();
            SetBool(door, "_isLocked", true);
            SetEnum(door, "_movementType", (int)DoorMovementType.Slide);
            SetVector3(door, "_slideOffset", new Vector3(0f, 3.25f, 0f));
            SetString(door, "_lockedPrompt", "Cal restablir l'energia");
            SetString(door, "_openPrompt", "Obrir porta de seguretat");
            SetBool(door, "_allowEnemyOperation", false);
            NavMeshObstacle obstacle = gate.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.size = Vector3.one;
            obstacle.carving = true;
            return door;
        }

        private static void CreatePowerConsole(Vector3 position, Door door)
        {
            GameObject console = GameObject.CreatePrimitive(PrimitiveType.Cube);
            console.name = "EmergencyGenerator";
            console.transform.position = position;
            console.transform.localScale = new Vector3(.8f, 1.3f, .55f);
            SurvivalPowerConsole power = console.AddComponent<SurvivalPowerConsole>();
            SetReference(power, "_controlledDoor", door);
        }

        private static void CreateLighting()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.055f, .06f, .065f);
            GameObject lightObject = new GameObject("Directional Light");
            Light directional = lightObject.AddComponent<Light>();
            directional.type = LightType.Directional;
            directional.intensity = .08f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            for (int i = -1; i <= 1; i++)
            {
                GameObject lamp = new GameObject($"EmergencyLamp_{i + 2}");
                lamp.transform.position = new Vector3(0f, 3f, i * 11f);
                Light point = lamp.AddComponent<Light>();
                point.type = LightType.Point;
                point.color = i == 0 ? new Color(.6f, .12f, .08f) : new Color(.2f, .28f, .4f);
                point.intensity = i == 0 ? 2.2f : 1.4f;
                point.range = 8f;
            }
        }

        private static GameObject CreateCube(Transform parent, string name, Vector3 position, Vector3 scale)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent);
            cube.transform.position = position;
            cube.transform.localScale = scale;
            return cube;
        }

        private static GameObject CreateChildCube(Transform parent, string name, Vector3 localPosition, Vector3 localScale)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = localPosition;
            cube.transform.localScale = localScale;
            return cube;
        }

        private static void UseSurvivalProfileAndDemoStart()
        {
            GenreFeatureSettings settings = AssetDatabase.LoadAssetAtPath<GenreFeatureSettings>(Root + "/Resources/GenreFeatureSettings.asset");
            if (settings != null) { settings.SetProfile(GameGenre.SurvivalHorror); EditorUtility.SetDirty(settings); }
            GameFlowSettings flow = AssetDatabase.LoadAssetAtPath<GameFlowSettings>(Root + "/Resources/GameFlowSettings.asset");
            if (flow != null) SetString(flow, "_firstGameplayScene", "SurvivalHorrorDemo");
        }

        private static void AddSceneToBuildSettings(string path)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (scenes.Exists(entry => entry.path == path)) return;
            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void EnsureFolder(string path)
        {
            string[] segments = path.Split('/');
            string current = segments[0];
            for (int i = 1; i < segments.Length; i++)
            {
                string next = current + "/" + segments[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, segments[i]);
                current = next;
            }
        }

        private static void SetReference(Object target, string property, Object value) => SetSerialized(target, property, p => p.objectReferenceValue = value);
        private static void SetString(Object target, string property, string value) => SetSerialized(target, property, p => p.stringValue = value);
        private static void SetBool(Object target, string property, bool value) => SetSerialized(target, property, p => p.boolValue = value);
        private static void SetInt(Object target, string property, int value) => SetSerialized(target, property, p => p.intValue = value);
        private static void SetFloat(Object target, string property, float value) => SetSerialized(target, property, p => p.floatValue = value);
        private static void SetEnum(Object target, string property, int value) => SetSerialized(target, property, p => p.enumValueIndex = value);
        private static void SetVector3(Object target, string property, Vector3 value) => SetSerialized(target, property, p => p.vector3Value = value);

        private static void SetObjectArray(Object target, string property, Object[] values)
        {
            SetSerialized(target, property, p =>
            {
                p.arraySize = values.Length;
                for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            });
        }

        private static void SetSerialized(Object target, string property, System.Action<SerializedProperty> setter)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty found = serialized.FindProperty(property);
            if (found == null) { Debug.LogError($"Missing serialized property {target.GetType().Name}.{property}"); return; }
            setter(found);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }
    }
}
#endif
