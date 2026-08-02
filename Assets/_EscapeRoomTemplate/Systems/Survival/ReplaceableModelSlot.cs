using UnityEngine;

namespace EscapeRoomRevolt.Systems.Survival
{
    /// <summary>
    /// Keeps gameplay components on the prefab root while allowing its visual model to be
    /// replaced by assigning a model prefab. The primitive placeholder is hidden automatically.
    /// </summary>
    public sealed class ReplaceableModelSlot : MonoBehaviour
    {
        [Header("Visual replacement")]
        [Tooltip("Assign any model-only prefab here. It will be instantiated below ModelSocket at runtime.")]
        [SerializeField] private GameObject _modelPrefab;
        [SerializeField] private Transform _modelSocket;
        [SerializeField] private GameObject _placeholderVisual;

        private GameObject _modelInstance;

        public GameObject ModelPrefab => _modelPrefab;
        public Transform ModelSocket => _modelSocket != null ? _modelSocket : transform;

        private void Awake() => RefreshModel();

        public void SetModel(GameObject modelPrefab)
        {
            _modelPrefab = modelPrefab;
            RefreshModel();
        }

        [ContextMenu("Refresh Runtime Model")]
        public void RefreshModel()
        {
            if (_modelInstance != null) Destroy(_modelInstance);

            bool hasReplacement = _modelPrefab != null;
            if (_placeholderVisual != null) _placeholderVisual.SetActive(!hasReplacement);
            if (!hasReplacement) return;

            _modelInstance = Instantiate(_modelPrefab, ModelSocket);
            _modelInstance.name = $"{_modelPrefab.name}_Visual";
            _modelInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }
    }
}
