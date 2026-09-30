using UnityEngine;

namespace EscapeRoomRevolt.Core.Settings
{
    /// <summary>
    /// Enables selected optional features for the current scene without changing
    /// the project's global genre profile. Useful for showcase scenes that
    /// demonstrate one optional mechanic alongside shared Escape Room systems.
    /// </summary>
    [DefaultExecutionOrder(-150)]
    public sealed class SceneFeatureOverride : MonoBehaviour
    {
        [SerializeField] private OptionalGameFeature _enabledFeatures = OptionalGameFeature.Flashlight;

        public OptionalGameFeature EnabledFeatures => _enabledFeatures;

        private void OnEnable()
        {
            GameFeatures.RegisterSceneOverride(this);
        }

        private void OnDisable()
        {
            GameFeatures.UnregisterSceneOverride(this);
        }

        private void OnDestroy()
        {
            GameFeatures.UnregisterSceneOverride(this);
        }
    }
}