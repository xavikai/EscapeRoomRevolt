using EscapeRoomRevolt.Core.Settings;
using UnityEngine;
using UnityEngine.UIElements;

namespace EscapeRoomRevolt.UI.Toolkit
{
    /// <summary>Mount after every UIDocument has enabled, before binding optional presenters.</summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class SurvivalHUDMount : MonoBehaviour
    {
        private void Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            VisualTreeAsset template = Resources.Load<VisualTreeAsset>("SurvivalHUD");
            if (root == null || template == null) { Debug.LogError("Survival HUD document or template is missing."); return; }
            if (root.Q("sanity-hud") == null) template.CloneTree(root.Q("hud-layer") ?? root);
            if (GameFeatures.IsEnabled(OptionalGameFeature.Sanity) && GetComponent<SanityHUDController>() == null)
                gameObject.AddComponent<SanityHUDController>();
            const OptionalGameFeature hud = OptionalGameFeature.PlayerVitals | OptionalGameFeature.NightVision | OptionalGameFeature.EvidenceRecording;
            if (((GameFeatures.ActiveFeatures & hud) != 0 || GameFeatures.Genre == GameGenre.SurvivalHorror)
                && GetComponent<SurvivalHUDController>() == null)
                gameObject.AddComponent<SurvivalHUDController>();
        }
    }
}
