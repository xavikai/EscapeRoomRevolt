using EscapeRoomRevolt.Core.Settings;
using EscapeRoomRevolt.Core.Localization;
using EscapeRoomRevolt.Systems.Survival;
using UnityEngine;
using UnityEngine.UIElements;
using static EscapeRoomRevolt.Core.Localization.LocalizationService;

namespace EscapeRoomRevolt.UI.Toolkit
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class SanityHUDController : MonoBehaviour
    {
        private VisualElement _sanityHud;
        private VisualElement _sanityFill;
        private Label _sanityPercent;
        private Label _sanityState;
        private SanityController _sanity;
        private LocalizationService _localization;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            _sanityHud = root?.Q("sanity-hud");
            _sanityFill = root?.Q("sanity-fill");
            _sanityPercent = root?.Q<Label>("sanity-percent");
            _sanityState = root?.Q<Label>("sanity-state");
            RefreshSanity();
        }

        private void Update()
        {
            SanityController current = GameFeatures.IsEnabled(OptionalGameFeature.Sanity) ? SanityController.Instance : null;
            if (_sanity != current)
            {
                Unbind();
                _sanity = current;
                if (_sanity != null)
                {
                    _sanity.Changed += OnSanityChanged;
                    _sanity.StageChanged += OnSanityStageChanged;
                }
                RefreshSanity();
            }
            if (_localization != LocalizationService.Instance)
            {
                if (_localization != null) _localization.LanguageChanged -= RefreshSanity;
                _localization = LocalizationService.Instance;
                if (_localization != null) _localization.LanguageChanged += RefreshSanity;
                RefreshSanity();
            }
        }

        private void OnDisable()
        {
            Unbind();
            if (_localization != null) _localization.LanguageChanged -= RefreshSanity;
            _localization = null;
            SetVisible(_sanityHud, false);
        }
        private void Unbind()
        {
            if (_sanity != null)
            {
                _sanity.Changed -= OnSanityChanged;
                _sanity.StageChanged -= OnSanityStageChanged;
            }
            _sanity = null;
        }

        private void OnSanityChanged(float _) => RefreshSanity();
        private void OnSanityStageChanged(SanityStage _) => RefreshSanity();

        private void RefreshSanity()
        {
            if (_sanityHud == null) return;
            bool visible = GameFeatures.IsEnabled(OptionalGameFeature.Sanity) && _sanity != null;
            SetVisible(_sanityHud, visible);
            if (!visible) return;

            float normalized = Mathf.Clamp01(_sanity.Normalized);
            if (_sanityFill != null) _sanityFill.style.width = Length.Percent(normalized * 100f);
            if (_sanityPercent != null) _sanityPercent.text = $"{Mathf.RoundToInt(normalized * 100f)}%";
            if (_sanityState != null) _sanityState.text = _sanity.Stage switch
            {
                SanityStage.Uneasy => Tr("INQUIETO"),
                SanityStage.Distressed => Tr("ALTERADO"),
                SanityStage.Critical => Tr("CRÍTICO"),
                _ => Tr("ESTABLE")
            };
            _sanityHud.RemoveFromClassList("sanity-hud--uneasy");
            _sanityHud.RemoveFromClassList("sanity-hud--distressed");
            _sanityHud.RemoveFromClassList("sanity-hud--critical");
            if (_sanity.Stage == SanityStage.Uneasy) _sanityHud.AddToClassList("sanity-hud--uneasy");
            else if (_sanity.Stage == SanityStage.Distressed) _sanityHud.AddToClassList("sanity-hud--distressed");
            else if (_sanity.Stage == SanityStage.Critical) _sanityHud.AddToClassList("sanity-hud--critical");
        }


        private static void SetVisible(VisualElement element, bool visible)
        {
            if (element != null) element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
