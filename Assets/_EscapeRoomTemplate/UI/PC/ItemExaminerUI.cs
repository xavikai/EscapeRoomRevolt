using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using EscapeRoomRevolt.Systems.Inventory;

namespace EscapeRoomRevolt.UI.PC
{
    /// <summary>
    /// Handles the UI panel for examining a 3D item.
    /// Listens for mouse drag events to rotate the 3D model spawned in the ExamineChamber.
    /// Listens for clicks to combine with the active hotbar item.
    /// </summary>
    public class ItemExaminerUI : MonoBehaviour, IDragHandler, IScrollHandler, IPointerClickHandler
    {
        [Header("Settings")]
        [SerializeField] private float _rotationSpeed = 0.5f;
        [SerializeField] private float _zoomSpeed = 0.1f;
        [SerializeField] private float _minZoom = 0.5f;
        [SerializeField] private float _maxZoom = 4f;

        private GameObject _currentModel;
        private InventoryItemData _dataBeingExamined;
        private float _currentZoom = 1f;
        private Vector3 _initialScale = Vector3.one;

        private void OnDisable()
        {
            if (_currentModel != null)
                Destroy(_currentModel);
            _dataBeingExamined = null;
        }

        public void Show(InventoryItemData dataToExamine)
        {
            gameObject.SetActive(true);

            if (_currentModel != null)
                Destroy(_currentModel);

            _dataBeingExamined = dataToExamine;

            if (ExamineChamber.Instance == null)
            {
                Debug.LogError("[ItemExaminerUI] ExamineChamber is missing from the scene! Cannot display 3D model.");
                return;
            }

            if (dataToExamine != null && dataToExamine.WorldPrefab != null)
            {
                _currentModel = Instantiate(dataToExamine.WorldPrefab, ExamineChamber.Instance.SpawnPoint);
                _currentModel.transform.localPosition = Vector3.zero;
                _initialScale = _currentModel.transform.localScale;
                _currentZoom = 1f;
                
                var components = _currentModel.GetComponentsInChildren<MonoBehaviour>();
                foreach (var comp in components) Destroy(comp);
                
                var colliders = _currentModel.GetComponentsInChildren<Collider>();
                foreach (var col in colliders) Destroy(col);
                
                SetLayerRecursively(_currentModel, LayerMask.NameToLayer("Examine"));
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_currentModel == null) return;

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

        public void OnPointerClick(PointerEventData eventData)
        {
            // Only respond to click if not dragging
            if (eventData.dragging) return;

            if (_dataBeingExamined != null && InventoryManager.Instance != null)
            {
                bool success = InventoryManager.Instance.TryCombineWithActive(_dataBeingExamined.ItemId);
                if (success)
                {
                    // Close the examiner since the item was either destroyed or changed
                    UIManager.Instance.CloseItemExaminer();
                }
                else
                {
                    // Optional: play error sound or jiggle the model
                }
            }
        }
        
        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (newLayer < 0) return; 
            
            obj.layer = newLayer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }
    }
}
