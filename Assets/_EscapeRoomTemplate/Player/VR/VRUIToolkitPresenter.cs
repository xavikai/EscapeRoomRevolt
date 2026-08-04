using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using EscapeRoomRevolt.Core;

namespace EscapeRoomRevolt.Player.VR
{
    /// <summary>
    /// Converts the framework's existing UI Toolkit documents to Unity 6 native world-space panels at runtime.
    /// The original PanelSettings assets remain untouched, so the same prefabs still work on desktop.
    /// </summary>
    public sealed class VRUIToolkitPresenter : MonoBehaviour
    {
        [SerializeField] private Vector2 _referenceSize = new Vector2(1600f, 900f);
        [SerializeField] private float _distanceFromHead = 1.25f;
        [SerializeField] private float _worldScale = .001f;
        [SerializeField] private Vector3 _localOffset = new Vector3(0f, -.05f, 0f);

        private readonly List<PanelSettings> _runtimePanelSettings = new List<PanelSettings>();
        private Transform _head;

        private IEnumerator Start()
        {
            _head = GetComponent<VRPlayerPlatformAdapter>()?.Head;
            yield return null;
            ConfigureDocuments();
        }

        private void ConfigureDocuments()
        {
            if (_head == null) return;
            UIDocument[] documents = FindObjectsByType<UIDocument>(FindObjectsInactive.Include);
            foreach (UIDocument document in documents)
            {
                if (document == null || document.panelSettings == null) continue;
                PanelSettings runtimeSettings = Instantiate(document.panelSettings);
                runtimeSettings.name = document.panelSettings.name + "_VR_Runtime";
                runtimeSettings.renderMode = PanelRenderMode.WorldSpace;
                _runtimePanelSettings.Add(runtimeSettings);
                document.panelSettings = runtimeSettings;
                document.worldSpaceSizeMode = UIDocument.WorldSpaceSizeMode.Fixed;
                document.worldSpaceSize = _referenceSize;

                Transform panel = document.transform;
                panel.SetParent(_head, false);
                panel.localPosition = _localOffset + Vector3.forward * _distanceFromHead;
                panel.localRotation = Quaternion.identity;
                panel.localScale = Vector3.one * _worldScale;

                BoxCollider collider = document.GetComponent<BoxCollider>();
                if (collider == null) collider = document.gameObject.AddComponent<BoxCollider>();
                collider.size = new Vector3(_referenceSize.x, _referenceSize.y, 1f);
                collider.center = Vector3.zero;

                VRUIPanelColliderController inputGate = document.GetComponent<VRUIPanelColliderController>();
                if (inputGate == null) inputGate = document.gameObject.AddComponent<VRUIPanelColliderController>();
                bool isMenu = document.GetComponent<IMenuPanel>() != null;
                inputGate.Configure(isMenu
                    ? VRUIPanelColliderController.PanelKind.Menu
                    : VRUIPanelColliderController.PanelKind.Gameplay);

                // A desktop crosshair is uncomfortable and misleading in a head-tracked display.
                VisualElement crosshair = document.rootVisualElement?.Q("crosshair");
                if (crosshair != null) crosshair.style.display = DisplayStyle.None;
            }
        }

        private void OnDestroy()
        {
            foreach (PanelSettings settings in _runtimePanelSettings)
                if (settings != null) Destroy(settings);
            _runtimePanelSettings.Clear();
        }
    }
}
