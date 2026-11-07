using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Feedback
{
    /// <summary>
    /// Switches a prop's emission on and off from a UnityEvent — a fuse that lights up once its
    /// circuit is live, a panel LED, a rune that starts glowing.
    ///
    /// Works on instanced materials rather than a MaterialPropertyBlock on purpose: a block can set
    /// _EmissionColor but cannot enable the _EMISSION shader keyword, so a material authored with
    /// emission off would stay dark no matter what colour it was handed. This costs one material
    /// instance per renderer, which is the right trade for a handful of props that need to light up.
    /// </summary>
    public sealed class EmissiveToggle : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("Renderers to light up. Leave empty to use every renderer under this object — which "
               + "is what you want when the visual comes from a model prefab swapped in at runtime.")]
        [SerializeField] private Renderer[] _renderers;

        [Header("Emission")]
        [Tooltip("The lit colour. Intensity lives in the HDR values: push it past 1 for a glow the bloom picks up.")]
        [SerializeField, ColorUsage(false, true)] private Color _emissiveColor = new Color(4f, 3.3f, 1.8f);
        [Tooltip("Seconds to ramp up or down. 0 switches instantly, which suits a hard electrical snap; a short ramp suits something warming up.")]
        [SerializeField, Min(0f)] private float _fadeDuration = .15f;

        [Header("Optional light")]
        [Tooltip("A light switched together with the emission. Emission alone only makes the surface bright — it casts nothing onto the props around it.")]
        [SerializeField] private Light _light;

        [Header("Startup")]
        [Tooltip("Whether the prop begins lit. Leave off for anything that lights up as a reward.")]
        [SerializeField] private bool _startLit;

        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private Material[] _materials;
        private float _baseLightIntensity = 1f;
        private Coroutine _fade;
        private float _level;
        private bool _stateSet;

        /// <summary>True while the prop is lit (or on its way to lit).</summary>
        public bool IsLit { get; private set; }

        private void Start()
        {
            // Not Awake: when the visual is a model prefab that a ReplaceableModelSlot instantiates in
            // its own Awake, script order decides whether those renderers exist yet. By Start they do.
            if (!_stateSet) SetLit(_startLit, instant: true);
        }

        private void OnDestroy()
        {
            if (_materials == null) return;
            foreach (Material material in _materials)
                if (material != null) Destroy(material);
        }

        /// <summary>Lights the prop. Wire this to PieceSocketReceiver.OnPieceLocked.</summary>
        public void TurnOn() => SetLit(true);

        /// <summary>Darkens the prop again.</summary>
        public void TurnOff() => SetLit(false);

        public void Toggle() => SetLit(!IsLit);

        /// <summary>Jumps straight to the lit look with no ramp — for restoring a save, where the glow should already be there rather than animating on at load.</summary>
        public void TurnOnInstantly() => SetLit(true, instant: true);

        public void SetLit(bool lit) => SetLit(lit, instant: false);

        public void SetLit(bool lit, bool instant)
        {
            ResolveMaterials();
            IsLit = lit;
            _stateSet = true;

            if (_fade != null) { StopCoroutine(_fade); _fade = null; }

            float target = lit ? 1f : 0f;
            // A coroutine cannot start on an inactive object, and a prop lit while switched off should
            // still be lit when it comes back — so fall back to snapping rather than dropping the call.
            if (instant || _fadeDuration <= 0f || !isActiveAndEnabled) ApplyLevel(target);
            else _fade = StartCoroutine(FadeTo(target));
        }

        /// <summary>
        /// Re-reads the renderers, for when the visual itself was replaced after this component
        /// already resolved its materials — a ReplaceableModelSlot.SetModel at runtime leaves the
        /// cached materials pointing at a model that no longer exists.
        /// </summary>
        [ContextMenu("Rescan Renderers")]
        public void Rescan()
        {
            _materials = null;
            ResolveMaterials();
            ApplyLevel(_level);
        }

        private void ResolveMaterials()
        {
            if (_materials != null) return;

            if (_light != null) _baseLightIntensity = _light.intensity;

            Renderer[] targets = _renderers != null && _renderers.Length > 0
                ? _renderers
                : GetComponentsInChildren<Renderer>(true);

            List<Material> materials = new List<Material>();
            foreach (Renderer renderer in targets)
            {
                if (renderer == null) continue;
                foreach (Material material in renderer.materials)
                {
                    material.EnableKeyword("_EMISSION");
                    material.globalIlluminationFlags &= ~MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                    materials.Add(material);
                }
            }
            _materials = materials.ToArray();
        }

        private IEnumerator FadeTo(float target)
        {
            float start = _level;
            float elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                ApplyLevel(Mathf.Lerp(start, target, elapsed / _fadeDuration));
                yield return null;
            }
            ApplyLevel(target);
            _fade = null;
        }

        private void ApplyLevel(float level)
        {
            _level = Mathf.Clamp01(level);

            Color color = _emissiveColor * _level;
            foreach (Material material in _materials)
                if (material != null) material.SetColor(EmissionColorId, color);

            if (_light == null) return;
            _light.intensity = _baseLightIntensity * _level;
            _light.enabled = _level > 0f;
        }
    }
}
