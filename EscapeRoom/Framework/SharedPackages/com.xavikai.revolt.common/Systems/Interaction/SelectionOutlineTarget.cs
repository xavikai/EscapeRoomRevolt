using System.Collections.Generic;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Interaction
{
    /// <summary>
    /// Marks only the focused object's renderers for SelectionMaskOutlineFeature.
    /// It never changes shared materials, so it is safe for prefab instances.
    /// </summary>
    public sealed class SelectionOutlineTarget : MonoBehaviour
    {
        // Bit 30 is reserved by the framework and is independent from GameObject layers.
        public const uint RenderingLayerMask = 1u << 30;

        [SerializeField] private Renderer[] _renderers;
        private readonly Dictionary<Renderer, uint> _originalMasks = new();

        private void Awake()
        {
            if (_renderers == null || _renderers.Length == 0)
                _renderers = GetComponentsInChildren<Renderer>(true);
        }

        private void OnDisable() => SetHighlighted(false);

        public void SetHighlighted(bool highlighted)
        {
            foreach (var renderer in _renderers)
            {
                if (renderer == null) continue;
                if (highlighted)
                {
                    if (!_originalMasks.ContainsKey(renderer)) _originalMasks[renderer] = renderer.renderingLayerMask;
                    renderer.renderingLayerMask |= RenderingLayerMask;
                }
                else if (_originalMasks.TryGetValue(renderer, out uint originalMask))
                {
                    renderer.renderingLayerMask = originalMask;
                    _originalMasks.Remove(renderer);
                }
            }
        }
    }
}
