using EscapeRoomRevolt.Core;
using EscapeRoomRevolt.Core.Settings;
using UnityEngine;
using UnityEngine.UIElements;

namespace EscapeRoomRevolt.UI.Toolkit
{
    public sealed class SurvivalUIModule : GameModule
    {
        private static readonly SurvivalUIModule Module = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register() => GameModules.Register(Module);

        public override void InitializeScene(GameObject playerRoot)
        {
            const OptionalGameFeature hudFeatures = OptionalGameFeature.PlayerVitals |
                OptionalGameFeature.NightVision | OptionalGameFeature.EvidenceRecording;
            bool needsHUD = (GameFeatures.ActiveFeatures & hudFeatures) != 0 || GameFeatures.Genre == GameGenre.SurvivalHorror;
            bool needsSanity = GameFeatures.IsEnabled(OptionalGameFeature.Sanity);
            if (!needsHUD && !needsSanity) return;

            foreach (GameplayUIController ui in Object.FindObjectsByType<GameplayUIController>(FindObjectsSortMode.None))
            {
                if (ui.GetComponent<SurvivalHUDMount>() == null)
                    ui.gameObject.AddComponent<SurvivalHUDMount>();
            }
        }
    }
}
