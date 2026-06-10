using UnityEngine;
using System.Collections.Generic;
using EscapeRoomRevolt.Core.Save;

namespace EscapeRoomRevolt.Systems.Interaction
{
    /// <summary>
    /// Base class for all interactable objects. Inherit from this and
    /// override OnInteract() to add specific behaviour.
    ///
    /// Example:
    ///   public class Door : InteractableBase { ... }
    ///   public class Note : InteractableBase { ... }
    /// </summary>
    public abstract class InteractableBase : MonoBehaviour, IInteractable, ISaveable
    {
        [Header("Interaction Settings")]
        [SerializeField] private string _interactionPrompt = "Interact";
        [SerializeField] private bool _canInteract = true;
        [SerializeField] private string _saveId = "";

        [Header("Visual Feedback (Outline)")]
        [SerializeField] private bool _enableOutline = true;
        [Tooltip("Assign a custom Outline Material here to override the global one. Leave empty to use the Global Outline Material from InteractionManager.")]
        [SerializeField] private Material _outlineMaterial;
        [Tooltip("Specific renderers to outline. If empty, it will auto-find child renderers (can cause issues on complex objects).")]
        [SerializeField] private Renderer[] _highlightRenderers;

        // ── Unity Lifecycle ──────────────────────────────────────────────────
        protected virtual void Awake()
        {
            if (_highlightRenderers == null || _highlightRenderers.Length == 0)
            {
                // Auto-fetch if not specified, but this is dangerous for compound objects like keypads!
                _highlightRenderers = GetComponentsInChildren<Renderer>();
            }
        }

        protected virtual void Start()
        {
            // Set layer to 'Interactable' (Layer 6 by default)
            gameObject.layer = 6;
            
            SaveManager.Instance?.Register(this);
        }

        protected virtual void OnDestroy()
        {
            SaveManager.Instance?.Unregister(this);
        }

        // ── IInteractable ────────────────────────────────────────────────────
        public virtual string InteractionPrompt => _interactionPrompt;
        public virtual bool CanInteract => _canInteract && gameObject.activeInHierarchy;

        public void Interact()
        {
            if (!CanInteract) return;
            OnInteract();
        }

        public virtual void OnFocusEnter()
        {
            if (!_enableOutline) return;
            
            Material matToApply = _outlineMaterial != null ? _outlineMaterial : InteractionManager.Instance?.GlobalOutlineMaterial;
            if (matToApply == null) return;

            if (_highlightRenderers == null) return;

            foreach (var r in _highlightRenderers)
            {
                if (r == null) continue;
                
                // Add outline material to the end of the array using sharedMaterials to prevent instancing leaks
                var materials = r.sharedMaterials;
                var newMaterials = new Material[materials.Length + 1];
                materials.CopyTo(newMaterials, 0);
                newMaterials[materials.Length] = matToApply;
                r.sharedMaterials = newMaterials;
            }
        }

        public virtual void OnFocusExit()
        {
            if (!_enableOutline || _highlightRenderers == null) return;

            Material matToRemove = _outlineMaterial != null ? _outlineMaterial : InteractionManager.Instance?.GlobalOutlineMaterial;
            if (matToRemove == null) return;

            foreach (var r in _highlightRenderers)
            {
                if (r == null) continue;

                var materials = r.sharedMaterials;
                var newMaterials = new List<Material>();
                
                foreach (var mat in materials)
                {
                    // Keep all materials EXCEPT our dynamic outline material
                    if (mat != matToRemove)
                    {
                        newMaterials.Add(mat);
                    }
                }
                
                r.sharedMaterials = newMaterials.ToArray();
            }
        }

        // ── Abstract / Virtual ───────────────────────────────────────────────
        /// <summary>Implement the specific interaction logic here.</summary>
        protected abstract void OnInteract();

        // ── Protected Helpers ────────────────────────────────────────────────
        protected void SetInteractable(bool value) => _canInteract = value;

        /// <summary>Stable ID used by the Save system to persist state.</summary>
        public string SaveId => string.IsNullOrEmpty(_saveId) ? name : _saveId;

        public virtual string SaveData()
        {
            return "{}"; // Override in derived classes
        }

        public virtual void LoadData(string json)
        {
            // Override in derived classes
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(_saveId))
            {
                // Only generate for instances in the scene, not raw prefabs in the project
                if (!UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this) && !Application.isPlaying)
                {
                    _saveId = System.Guid.NewGuid().ToString();
                    UnityEditor.EditorUtility.SetDirty(this);
                }
            }
        }
#endif
    }
}
