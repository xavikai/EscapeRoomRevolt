#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using EscapeRoomRevolt.Player.PC;
using EscapeRoomRevolt.Player.VR;
using EscapeRoomRevolt.Systems.Equipment;
using EscapeRoomRevolt.Systems.Interaction;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

namespace EscapeRoomRevolt.EditorTools
{
    /// <summary>One-click authoring tools for the framework's vendor-neutral OpenXR template.</summary>
    public static class VRSetupTools
    {
        private const string Root = "Escape Room Framework/";
        private const string StarterRigPath =
            "Assets/Samples/XR Interaction Toolkit/3.3.0/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";
        private const string SimulatorPath =
            "Assets/Samples/XR Interaction Toolkit/3.3.0/XR Interaction Simulator/XR Interaction Simulator.prefab";
        private const string VignettePrefabPath =
            "Assets/Samples/XR Interaction Toolkit/3.3.0/Starter Assets/TunnelingVignette/TunnelingVignette.prefab";
        private const string VrPrefabPath = "Assets/_EscapeRoomTemplate/Prefabs/Player_VR.prefab";
        private const string VrScenePath = "Assets/_EscapeRoomTemplate/Scenes/VRTemplate.unity";
        private const string ComfortAssetPath = "Assets/_EscapeRoomTemplate/Resources/VRComfortSettings.asset";
        private const string XrSettingsPath = "Assets/XR/XRGeneralSettingsPerBuildTarget.asset";
        private const string OpenXrLoaderType = "UnityEngine.XR.OpenXR.OpenXRLoader";

        [MenuItem(Root + "Setup/Build Complete VR Template", priority = 28)]
        public static void BuildCompleteVrTemplate()
        {
            ConfigureOpenXR();
            CreateVrPlayerPrefab();
            CreateVrTemplateScene();
            Debug.Log("[Escape Room Framework] VR template ready: OpenXR, Player_VR and VRTemplate scene were generated.");
        }

        [MenuItem(Root + "Setup/Configure OpenXR (PC + Android)", priority = 29)]
        public static void ConfigureOpenXR()
        {
            EnsureFolder("Assets/XR");
            XRGeneralSettingsPerBuildTarget perTarget =
                AssetDatabase.LoadAssetAtPath<XRGeneralSettingsPerBuildTarget>(XrSettingsPath);
            if (perTarget == null)
            {
                perTarget = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
                AssetDatabase.CreateAsset(perTarget, XrSettingsPath);
            }

            EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, perTarget, true);
            ConfigureTarget(perTarget, BuildTargetGroup.Standalone);
            ConfigureTarget(perTarget, BuildTargetGroup.Android);
            EditorUtility.SetDirty(perTarget);
            AssetDatabase.SaveAssets();
            Debug.Log("[Escape Room Framework] OpenXR loader assigned to Standalone and Android.");
        }

        [MenuItem(Root + "Setup/Create or Update VR Player Prefab", priority = 30)]
        public static void CreateVrPlayerPrefab()
        {
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(StarterRigPath);
            if (source == null)
                throw new InvalidOperationException(
                    "XRI Starter Assets are missing. Import the Starter Assets sample for XR Interaction Toolkit 3.3.0.");

            VRComfortSettings comfort = EnsureComfortAsset();
            GameObject rig = PrefabUtility.InstantiatePrefab(source) as GameObject;
            if (rig == null) throw new InvalidOperationException("Unity could not instantiate the official XRI rig.");

            rig.name = "Player_VR";
            try { rig.tag = "Player"; }
            catch (UnityException) { Debug.LogWarning("[VR Setup] The Player tag does not exist."); }

            XROrigin origin = rig.GetComponent<XROrigin>();
            Transform leftController = FindController(rig.transform, "left");
            Transform rightController = FindController(rig.transform, "right");
            Transform leftSocket = EnsureModelSocket(leftController);
            Transform rightSocket = EnsureModelSocket(rightController);

            VRPlayerPlatformAdapter adapter = GetOrAdd<VRPlayerPlatformAdapter>(rig);
            adapter.Configure(origin != null && origin.Camera != null ? origin.Camera.transform : null, leftController, rightController);
            GetOrAdd<VRUIToolkitPresenter>(rig);
            VRComfortController comfortController = GetOrAdd<VRComfortController>(rig);
            SetObjectReference(comfortController, "_settings", comfort);
            SetObjectReference(comfortController, "_vignette", CreateComfortVignette(origin));
            EquipmentController equipment = GetOrAdd<EquipmentController>(rig);
            SetObjectReference(equipment, "_equipmentSocket", rightSocket != null ? rightSocket : leftSocket);
            if (rightSocket != null && leftSocket != null)
                SetObjectReference(equipment, "_leftEquipmentSocket", leftSocket);
            GetOrAdd<PlayerInputHandler>(rig);

            PrefabUtility.SaveAsPrefabAsset(rig, VrPrefabPath);
            UnityEngine.Object.DestroyImmediate(rig);
            AssetDatabase.SaveAssets();
            Debug.Log("[Escape Room Framework] Player_VR rebuilt from Unity's complete XRI Starter Assets rig.");
        }

        [MenuItem(Root + "Setup/Create VR Template Scene", priority = 31)]
        public static void CreateVrTemplateScene()
        {
            Scene active = SceneManager.GetActiveScene();
            if (active.IsValid() && active.isDirty && !string.IsNullOrEmpty(active.path))
                EditorSceneManager.SaveScene(active);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "VRTemplate";

            InstantiatePrefab("Assets/_EscapeRoomTemplate/Prefabs/GameManager.prefab");
            InstantiatePrefab(VrPrefabPath);

            new GameObject("XR Interaction Manager").AddComponent<XRInteractionManager>();
            GameObject eventSystemObject = new GameObject("XR EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<XRUIInputModule>();

            GameObject lightObject = new GameObject("Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Teleport Floor";
            floor.transform.SetPositionAndRotation(new Vector3(0f, -.05f, 0f), Quaternion.identity);
            floor.transform.localScale = new Vector3(8f, .1f, 8f);
            floor.AddComponent<TeleportationArea>();

            CreateGrabTest(new Vector3(-.7f, .9f, 1.8f));
            CreateSwitchTest(new Vector3(.7f, 1.1f, 1.8f));

            GameObject simulatorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SimulatorPath);
            if (simulatorPrefab != null)
            {
                GameObject simulator = PrefabUtility.InstantiatePrefab(simulatorPrefab) as GameObject;
                if (simulator != null)
                {
                    simulator.name = "XR Interaction Simulator (enable for editor testing)";
                    simulator.SetActive(false);
                }
            }

            EditorSceneManager.SaveScene(scene, VrScenePath);
            AddSceneToBuildSettings(VrScenePath);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(VrScenePath);
            Debug.Log("[Escape Room Framework] VRTemplate scene created. Enable the simulator object to test without a headset.");
        }

        [MenuItem(Root + "Setup/Prepare Current Scene Interactables for VR", priority = 32)]
        public static void PrepareInteractables()
        {
            int converted = 0;
            int skipped = 0;
            foreach (MonoBehaviour behaviour in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include))
            {
                if (behaviour is not IInteractable) continue;
                Collider[] ownedColliders = CollectOwnedColliders(behaviour);
                if (ownedColliders.Length == 0) { skipped++; continue; }

                bool grabbable = behaviour.GetComponent<PhysicsGrabbable>() != null;
                XRBaseInteractable xrInteractable = grabbable
                    ? GetOrAdd<XRGrabInteractable>(behaviour.gameObject)
                    : GetOrAdd<XRSimpleInteractable>(behaviour.gameObject);
                XRBaseInteractable other = grabbable
                    ? behaviour.GetComponent<XRSimpleInteractable>()
                    : behaviour.GetComponent<XRGrabInteractable>();
                if (other != null) Undo.DestroyObjectImmediate(other);

                Undo.RecordObject(xrInteractable, "Configure VR interactable");
                xrInteractable.colliders.Clear();
                foreach (Collider owned in ownedColliders) xrInteractable.colliders.Add(owned);

                VRInteractionBridge bridge = GetOrAdd<VRInteractionBridge>(behaviour.gameObject);
                SetObjectReference(bridge, "_interactableSource", behaviour);
                SetBoolean(bridge, "_interactOnSelect", !grabbable);
                converted++;
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log($"[Escape Room Framework] Prepared {converted} shared interactable(s) for VR; skipped {skipped} without colliders.");
        }

        private static void ConfigureTarget(XRGeneralSettingsPerBuildTarget perTarget, BuildTargetGroup target)
        {
            if (!perTarget.HasSettingsForBuildTarget(target)) perTarget.CreateDefaultSettingsForBuildTarget(target);
            if (!perTarget.HasManagerSettingsForBuildTarget(target)) perTarget.CreateDefaultManagerSettingsForBuildTarget(target);
            XRGeneralSettings general = perTarget.SettingsForBuildTarget(target);
            general.InitManagerOnStart = true;
            XRPackageMetadataStore.AssignLoader(general.Manager, OpenXrLoaderType, target);
            ConfigureInteractionProfiles(target);
            EditorUtility.SetDirty(general);
            EditorUtility.SetDirty(general.Manager);
        }

        private static void ConfigureInteractionProfiles(BuildTargetGroup target)
        {
            OpenXRSettings settings = OpenXRSettings.GetSettingsForBuildTargetGroup(target);
            if (settings == null) return;

            var commonProfiles = new HashSet<string>
            {
                "KHRSimpleControllerProfile",
                "OculusTouchControllerProfile",
                "MetaQuestTouchPlusControllerProfile",
                "MetaQuestTouchProControllerProfile"
            };
            var pcProfiles = new HashSet<string>
            {
                "ValveIndexControllerProfile",
                "HTCViveControllerProfile",
                "MicrosoftMotionControllerProfile",
                "HPReverbG2ControllerProfile"
            };
            var developmentOnly = new HashSet<string>
            {
                "MockRuntime",
                "ConformanceAutomationFeature",
                "RuntimeDebuggerOpenXRFeature"
            };

            foreach (OpenXRFeature feature in settings.GetFeatures())
            {
                string name = feature.GetType().Name;
                if (commonProfiles.Contains(name)
                    || (target == BuildTargetGroup.Standalone && pcProfiles.Contains(name))
                    || (target == BuildTargetGroup.Android && name == "MetaQuestFeature"))
                    feature.enabled = true;
                else if (developmentOnly.Contains(name))
                    feature.enabled = false;
                EditorUtility.SetDirty(feature);
            }
            EditorUtility.SetDirty(settings);
        }

        /// <summary>Drops the XRI sample's tunneling vignette in front of the headset camera so continuous move/turn ease the FOV down instead of risking motion sickness.</summary>
        private static TunnelingVignetteController CreateComfortVignette(XROrigin origin)
        {
            if (origin == null || origin.Camera == null) return null;

            GameObject vignettePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(VignettePrefabPath);
            if (vignettePrefab == null)
            {
                Debug.LogWarning("[VR Setup] Tunneling Vignette sample not found; comfort vignette will stay disabled. Import the Starter Assets sample for XR Interaction Toolkit 3.3.0.");
                return null;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(vignettePrefab) as GameObject;
            if (instance == null) return null;

            instance.transform.SetParent(origin.Camera.transform, false);
            return instance.GetComponent<TunnelingVignetteController>();
        }

        private static VRComfortSettings EnsureComfortAsset()
        {
            VRComfortSettings settings = AssetDatabase.LoadAssetAtPath<VRComfortSettings>(ComfortAssetPath);
            if (settings != null) return settings;
            settings = ScriptableObject.CreateInstance<VRComfortSettings>();
            AssetDatabase.CreateAsset(settings, ComfortAssetPath);
            return settings;
        }

        private static void CreateGrabTest(Vector3 position)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "VR Grab Test (replace ModelSocket visuals)";
            cube.transform.position = position;
            cube.transform.localScale = Vector3.one * .25f;
            cube.AddComponent<Rigidbody>();
            PhysicsGrabbable gameplay = cube.AddComponent<PhysicsGrabbable>();
            XRGrabInteractable xr = cube.AddComponent<XRGrabInteractable>();
            xr.colliders.Clear();
            xr.colliders.Add(cube.GetComponent<Collider>());
            VRInteractionBridge bridge = cube.AddComponent<VRInteractionBridge>();
            SetObjectReference(bridge, "_interactableSource", gameplay);
            SetBoolean(bridge, "_interactOnSelect", false);
        }

        private static void CreateSwitchTest(Vector3 position)
        {
            GameObject toggle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            toggle.name = "VR Interaction Test";
            toggle.transform.position = position;
            toggle.transform.localScale = new Vector3(.18f, .35f, .12f);
            InteractableToggle gameplay = toggle.AddComponent<InteractableToggle>();
            SetObjectReference(gameplay, "_visualTransform", toggle.transform);
            XRSimpleInteractable xr = toggle.AddComponent<XRSimpleInteractable>();
            xr.colliders.Clear();
            xr.colliders.Add(toggle.GetComponent<Collider>());
            VRInteractionBridge bridge = toggle.AddComponent<VRInteractionBridge>();
            SetObjectReference(bridge, "_interactableSource", gameplay);
            SetBoolean(bridge, "_interactOnSelect", true);
        }

        private static Collider[] CollectOwnedColliders(MonoBehaviour owner)
        {
            return owner.GetComponentsInChildren<Collider>(true)
                .Where(collider => IsOwnedBy(collider.transform, owner)).Distinct().ToArray();
        }

        private static bool IsOwnedBy(Transform colliderTransform, MonoBehaviour owner)
        {
            Transform cursor = colliderTransform;
            while (cursor != null)
            {
                foreach (MonoBehaviour behaviour in cursor.GetComponents<MonoBehaviour>())
                    if (behaviour is IInteractable) return behaviour == owner;
                if (cursor == owner.transform) break;
                cursor = cursor.parent;
            }
            return false;
        }

        private static Transform FindController(Transform root, string side)
        {
            return root.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(child =>
                {
                    string lower = child.name.ToLowerInvariant();
                    return lower.Contains(side) && lower.Contains("controller")
                        && !lower.Contains("model") && !lower.Contains("stabilized");
                });
        }

        private static Transform EnsureModelSocket(Transform controller)
        {
            if (controller == null) return null;
            Transform existing = controller.Find("ModelSocket");
            if (existing != null) return existing;
            GameObject socket = new GameObject("ModelSocket");
            socket.transform.SetParent(controller, false);
            return socket.transform;
        }

        private static GameObject InstantiatePrefab(string path)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) throw new InvalidOperationException($"Required prefab not found: {path}");
            return PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        }

        private static T GetOrAdd<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }

        private static void SetObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null) throw new MissingFieldException(target.GetType().Name, propertyName);
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetBoolean(UnityEngine.Object target, string propertyName, bool value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null) throw new MissingFieldException(target.GetType().Name, propertyName);
            property.boolValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = path.Substring(0, path.LastIndexOf('/'));
            string name = path.Substring(path.LastIndexOf('/') + 1);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static void AddSceneToBuildSettings(string path)
        {
            List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.All(entry => entry.path != path))
                scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
#endif
