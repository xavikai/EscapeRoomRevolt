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

            if (Settings.GameFeatures.IsEnabled(Settings.OptionalGameFeature.PlayerVitals)
                && GameObject.FindAnyObjectByType<EscapeRoomRevolt.Systems.Survival.SurvivalDifficultyService>() == null)
                new GameObject("SurvivalDifficultyService").AddComponent<EscapeRoomRevolt.Systems.Survival.SurvivalDifficultyService>();

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

            if (playerRoot != null && Settings.GameFeatures.IsEnabled(Settings.OptionalGameFeature.PlayerVitals)
                && playerRoot.GetComponent<EscapeRoomRevolt.Systems.Survival.PlayerVitals>() == null)
                playerRoot.AddComponent<EscapeRoomRevolt.Systems.Survival.PlayerVitals>();

            if (playerRoot != null && Settings.GameFeatures.IsEnabled(Settings.OptionalGameFeature.PlayerVitals)
                && playerRoot.GetComponent<EscapeRoomRevolt.Systems.Survival.PlayerDamageFeedbackRelay>() == null)
                playerRoot.AddComponent<EscapeRoomRevolt.Systems.Survival.PlayerDamageFeedbackRelay>();

            if (playerRoot != null && Settings.GameFeatures.IsEnabled(Settings.OptionalGameFeature.Traversal)
                && playerRoot.GetComponent<EscapeRoomRevolt.Systems.Survival.TraversalController>() == null)
                playerRoot.AddComponent<EscapeRoomRevolt.Systems.Survival.TraversalController>();

            if (playerRoot != null && Settings.GameFeatures.IsEnabled(Settings.OptionalGameFeature.AdvancedEvasion)
                && playerRoot.GetComponent<EscapeRoomRevolt.Systems.Survival.EvasionController>() == null)
                playerRoot.AddComponent<EscapeRoomRevolt.Systems.Survival.EvasionController>();

            if (Settings.GameFeatures.IsEnabled(Settings.OptionalGameFeature.EvidenceRecording)
                && GameObject.FindAnyObjectByType<EscapeRoomRevolt.Systems.Survival.EvidenceJournal>() == null)
                new GameObject("EvidenceJournal").AddComponent<EscapeRoomRevolt.Systems.Survival.EvidenceJournal>();

            if (playerRoot != null && Settings.GameFeatures.IsEnabled(Settings.OptionalGameFeature.EnemyAI)
                && playerRoot.GetComponent<EscapeRoomRevolt.Systems.Survival.PlayerVisibility>() == null)
                playerRoot.AddComponent<EscapeRoomRevolt.Systems.Survival.PlayerVisibility>();

            if (Settings.GameFeatures.IsEnabled(Settings.OptionalGameFeature.EnemyAI)
                && GameObject.FindAnyObjectByType<EscapeRoomRevolt.Systems.Survival.ChaseDirector>() == null)
                new GameObject("ChaseDirector").AddComponent<EscapeRoomRevolt.Systems.Survival.ChaseDirector>();

            if (Settings.GameFeatures.IsEnabled(Settings.OptionalGameFeature.Checkpoints)
                && GameObject.FindAnyObjectByType<EscapeRoomRevolt.Systems.Survival.CheckpointManager>() == null)
                new GameObject("CheckpointManager").AddComponent<EscapeRoomRevolt.Systems.Survival.CheckpointManager>();

            if (Settings.GameFeatures.IsEnabled(Settings.OptionalGameFeature.Sanity)
                && GameObject.FindAnyObjectByType<EscapeRoomRevolt.Systems.Survival.SanityController>() == null)
            {
                var sanityObject = new GameObject("SanityController");
                sanityObject.AddComponent<EscapeRoomRevolt.Systems.Survival.SanityController>();
            }

            if (playerRoot != null && Settings.GameFeatures.IsEnabled(Settings.OptionalGameFeature.Sanity)
                && playerRoot.GetComponent<EscapeRoomRevolt.Systems.Survival.SanityFeedbackController>() == null)
                playerRoot.AddComponent<EscapeRoomRevolt.Systems.Survival.SanityFeedbackController>();

            if (playerRoot != null && playerRoot.GetComponent<EscapeRoomRevolt.Systems.Survival.CameraShakeController>() == null)
                playerRoot.AddComponent<EscapeRoomRevolt.Systems.Survival.CameraShakeController>();

            if (playerRoot != null && Settings.GameFeatures.IsEnabled(Settings.OptionalGameFeature.Hiding)
                && playerRoot.GetComponent<EscapeRoomRevolt.Systems.Survival.HidingViewFeedback>() == null)
                playerRoot.AddComponent<EscapeRoomRevolt.Systems.Survival.HidingViewFeedback>();

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

            EscapeRoomRevolt.UI.Toolkit.GameplayUIController gameplayUI =
                GameObject.FindAnyObjectByType<EscapeRoomRevolt.UI.Toolkit.GameplayUIController>();
            if (gameplayUI != null && gameplayUI.GetComponent<EscapeRoomRevolt.UI.Toolkit.SurvivalHUDController>() == null)
                gameplayUI.gameObject.AddComponent<EscapeRoomRevolt.UI.Toolkit.SurvivalHUDController>();

            // Systems will be initialized here as they are implemented.
            // Order matters — add systems in dependency order:
            //
            // EPIC 03: InventoryManager
            // EPIC 04: PuzzleManager
            // EPIC 05: SaveManager
            // EPIC 06: UIManager
            //
            // Example (once implemented):
            //   GameContext.Inventory.Initialize();
            //   GameContext.SaveManager.Initialize();

            Log("Core systems initialized (AudioManager & EventBus ready).");
        }

        private void Log(string message)
        {
            if (_logInitialization)
                Debug.Log($"[Bootstrapper] {message}");
        }
    }
}
