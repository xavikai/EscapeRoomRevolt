using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using EscapeRoomRevolt.Systems.Inventory;

namespace EscapeRoomRevolt.UI.PC
{
    /// <summary>
    /// Handles the UI panel for examining a 3D item.
    /// Listens for mouse drag events to rotate the 3D model spawned in the ExamineChamber.
    /// Handles the UI panel for examining a 3D item.
    /// Listens for mouse drag events to rotate the 3D model spawned in the ExamineChamber.
    /// </summary>
    public class ItemExaminerUI : MonoBehaviour, IDragHandler, IScrollHandler
    {
        [Header("Settings")]
        [SerializeField] private float _rotationSpeed = 0.5f;
        [SerializeField] private float _zoomSpeed = 0.1f;
        [SerializeField] private float _minZoom = 0.5f;
        [SerializeField] private float _maxZoom = 4f;

        private GameObject _currentModel;
        private float _currentZoom = 1f;
        private Vector3 _initialScale = Vector3.one;

        private void OnDisable()
        {
            if (_currentModel != null)
                Destroy(_currentModel);
        }

        /// <summary>
        /// Opens the examiner panel and spawns the 3D model.
        /// </summary>
        public void Show(GameObject prefabToExamine)
        {
            gameObject.SetActive(true);

            // Clean up any old model
            if (_currentModel != null)
                Destroy(_currentModel);

            // Make sure the Examine Chamber exists in the scene
            if (ExamineChamber.Instance == null)
            {
                Debug.LogError("[ItemExaminerUI] ExamineChamber is missing from the scene! Cannot display 3D model.");
                return;
            }

            if (prefabToExamine != null)
            {
                // Instantiate the prefab inside the Examine Chamber
                _currentModel = Instantiate(prefabToExamine, ExamineChamber.Instance.SpawnPoint);
                _currentModel.transform.localPosition = Vector3.zero;
                _initialScale = _currentModel.transform.localScale;
                _currentZoom = 1f;
                
                // Remove scripts and colliders so it's just visual
                var components = _currentModel.GetComponentsInChildren<MonoBehaviour>();
                foreach (var comp in components) Destroy(comp);
                
                var colliders = _currentModel.GetComponentsInChildren<Collider>();
                foreach (var col in colliders) Destroy(col);
                
                // Set layer to 'Examine' (assuming Layer 7 or custom, usually done visually by the user, but we'll try to automate or rely on the prefab)
                // The easiest is to set the whole tree to the Examine layer so the ExamineCamera sees it.
                SetLayerRecursively(_currentModel, LayerMask.NameToLayer("Examine"));
            }
        }

        // UIManager handles closing this panel when Escape is pressed.

        public void OnDrag(PointerEventData eventData)
        {
            if (_currentModel == null) return;

            // Rotate the model based on mouse drag delta
            // X drag rotates around world Up (Y)
            // Y drag rotates around world Right (X)
            float rotX = eventData.delta.x * _rotationSpeed;
            float rotY = eventData.delta.y * _rotationSpeed;

            _currentModel.transform.Rotate(Vector3.up, -rotX, Space.World);
            _currentModel.transform.Rotate(Vector3.right, rotY, Space.World);
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (_currentModel == null) return;

            float scroll = eventData.scrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                _currentZoom += scroll * _zoomSpeed;
                _currentZoom = Mathf.Clamp(_currentZoom, _minZoom, _maxZoom);
                _currentModel.transform.localScale = _initialScale * _currentZoom;
            }
        }
        
        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (newLayer < 0) return; // Layer doesn't exist yet
            
            obj.layer = newLayer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }
    }
}
