using EscapeRoomRevolt.Core.Flow;
using UnityEngine;

namespace EscapeRoomRevolt.Core
{
    /// <summary>
    /// Scene entry point. Place one instance of this MonoBehaviour
    /// in every room scene. It initializes all core systems in the
    /// correct order before gameplay begins.
    ///
    /// Execution order: -100 (runs before any other script)
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class Bootstrapper : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool _logInitialization = true;

        private void Awake()
        {
            Log("Bootstrapper starting...");
            InitializeSystems();
            GameContext.MarkInitialized();
            Log("Bootstrapper complete. All systems ready.");
        }

        private void InitializeSystems()
        {
            GameFlowManager.EnsureInstance();
            Log($"Genre profile: {Settings.GameFeatures.Genre}; optional features: {Settings.GameFeatures.ActiveFeatures}.");

            EscapeRoomRevolt.Player.PC.PlayerMovement pcPlayer = GameObject.FindAnyObjectByType<EscapeRoomRevolt.Player.PC.PlayerMovement>();
            if (pcPlayer != null && pcPlayer.GetComponent<EscapeRoomRevolt.Player.PC.PCPlayerPlatformAdapter>() == null)
                pcPlayer.gameObject.AddComponent<EscapeRoomRevolt.Player.PC.PCPlayerPlatformAdapter>();
            EscapeRoomRevolt.Player.VR.VRPlayerPlatformAdapter vrPlayer =
                GameObject.FindAnyObjectByType<EscapeRoomRevolt.Player.VR.VRPlayerPlatformAdapter>();
            GameObject playerRoot = pcPlayer != null ? pcPlayer.gameObject : vrPlayer != null ? vrPlayer.gameObject : null;

            // Save must exist before scene objects execute their registration code.
            // Keeping it persistent also makes scene transitions and save slots reliable.
            if (GameObject.FindAnyObjectByType<EscapeRoomRevolt.Core.Save.SaveManager>() == null)
            {
                var saveObject = new GameObject("SaveManager");
                saveObject.AddComponent<EscapeRoomRevolt.Core.Save.SaveManager>();
            }

            GameModules.EnsureServices();

            if (GameObject.FindAnyObjectByType<EscapeRoomRevolt.Core.Settings.GameSettingsService>() == null)
            {
                var settingsObject = new GameObject("GameSettingsService");
                settingsObject.AddComponent<EscapeRoomRevolt.Core.Settings.GameSettingsService>();
            }

            if (GameObject.FindAnyObjectByType<EscapeRoomRevolt.Core.Input.InputRouter>() == null)
            {
                var inputObject = new GameObject("InputRouter");
                inputObject.AddComponent<EscapeRoomRevolt.Core.Input.InputRouter>();
            }

            if (playerRoot != null && playerRoot.GetComponent<EscapeRoomRevolt.Systems.Survival.CameraShakeController>() == null)
                playerRoot.AddComponent<EscapeRoomRevolt.Systems.Survival.CameraShakeController>();

            // PC-only: VR players already have real physical head movement, so this never runs there.
            if (pcPlayer != null && pcPlayer.GetComponent<EscapeRoomRevolt.Player.PC.HeadBobController>() == null)
                pcPlayer.gameObject.AddComponent<EscapeRoomRevolt.Player.PC.HeadBobController>();

            if (GameObject.FindAnyObjectByType<EscapeRoomRevolt.Core.Localization.LocalizationService>() == null)
            {
                GameObject localizationObj = new GameObject("LocalizationService");
                localizationObj.AddComponent<EscapeRoomRevolt.Core.Localization.LocalizationService>();
            }

            // Ensure AudioManager exists in the scene
            if (GameObject.FindAnyObjectByType<EscapeRoomRevolt.Systems.Audio.AudioManager>() == null)
            {
                GameObject amObj = new GameObject("AudioManager");
                amObj.AddComponent<EscapeRoomRevolt.Systems.Audio.AudioManager>();
            }

            // Ensure HintManager exists in the scene
            if (GameObject.FindAnyObjectByType<EscapeRoomRevolt.Systems.Hint.HintManager>() == null)
            {
                GameObject hmObj = new GameObject("HintManager");
                hmObj.AddComponent<EscapeRoomRevolt.Systems.Hint.HintManager>();
            }

            if (GameObject.FindAnyObjectByType<EscapeRoomRevolt.Systems.Inventory.ExamineHotspotRegistry>() == null)
            {
                GameObject hotspotObj = new GameObject("ExamineHotspotRegistry");
                hotspotObj.AddComponent<EscapeRoomRevolt.Systems.Inventory.ExamineHotspotRegistry>();
            }

            GameModules.InitializeScene(playerRoot);

            Log("Core systems initialized (AudioManager & EventBus ready).");
        }

        private void Log(string message)
        {
            if (_logInitialization)
                Debug.Log($"[Bootstrapper] {message}");
        }
    }
}
