using UnityEngine;

namespace EscapeRoomRevolt.Systems.Survival
{
    /// <summary>
    /// Keeps gameplay components on the prefab root while allowing its visual model to be
    /// replaced by assigning a model prefab. The primitive placeholder is hidden automatically.
    ///
    /// Runs in edit mode as well as at runtime, so assigning a model shows it immediately in the
    /// scene: a designer placing pieces has to see the real shape to judge scale and framing, and
    /// a preview that only appears in play mode means authoring blind. The preview instance is
    /// flagged DontSave, so it is never serialised into the scene and cannot be edited by mistake —
    /// the model prefab stays the single source of truth.
    /// </summary>
    [ExecuteAlways]
    public sealed class ReplaceableModelSlot : MonoBehaviour
    {
        [Header("Visual replacement")]
        [Tooltip("Assign any model-only prefab here. It will be instantiated below ModelSocket, replacing the placeholder.")]
        [SerializeField] private GameObject _modelPrefab;
        [Tooltip("Where the model is instantiated. Leave empty to use this object, which is what you want unless the model needs an offset the prefab itself cannot carry.")]
        [SerializeField] private Transform _modelSocket;
        [Tooltip("The primitive stand-in, hidden as soon as a model prefab is assigned.")]
        [SerializeField] private GameObject _placeholderVisual;

        private GameObject _modelInstance;

        public GameObject ModelPrefab => _modelPrefab;
        public Transform ModelSocket => _modelSocket != null ? _modelSocket : transform;

        private void Awake()
        {
            if (Application.isPlaying) RefreshModel();
        }

        private void OnEnable()
        {
            // The edit-mode preview does not survive a domain reload or a scene open, so it has to be
            // rebuilt — but not from inside OnEnable itself, where instantiating is unsafe.
            if (!Application.isPlaying) ScheduleEditorRefresh();
        }

        public void SetModel(GameObject modelPrefab)
        {
            _modelPrefab = modelPrefab;
            RefreshModel();
        }

        [ContextMenu("Refresh Model")]
        public void RefreshModel()
        {
            if (_modelInstance != null) DestroyAnywhere(_modelInstance);

            bool hasReplacement = _modelPrefab != null;
            if (_placeholderVisual != null) _placeholderVisual.SetActive(!hasReplacement);
            if (!hasReplacement) return;

            _modelInstance = Instantiate(_modelPrefab, ModelSocket);
            _modelInstance.name = $"{_modelPrefab.name}_Visual";
            _modelInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            if (!Application.isPlaying)
                _modelInstance.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
        }

        private static void DestroyAnywhere(Object target)
        {
            if (Application.isPlaying) Destroy(target);
            else DestroyImmediate(target);
        }

#if UNITY_EDITOR
        private void OnValidate() => ScheduleEditorRefresh();

        private void ScheduleEditorRefresh()
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null && !Application.isPlaying) RefreshModel();
            };
        }
#else
        private void ScheduleEditorRefresh() { }
#endif
    }
}
