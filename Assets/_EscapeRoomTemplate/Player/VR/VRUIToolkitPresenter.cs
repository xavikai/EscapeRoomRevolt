using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using EscapeRoomRevolt.Core;

namespace EscapeRoomRevolt.Player.VR
{
    /// <summary>
    /// Renders the framework's existing UI Toolkit documents to a RenderTexture and displays that
    /// texture on a 3D quad in front of the player's head, instead of Unity's native
    /// PanelRenderMode.WorldSpace — confirmed by direct testing that WorldSpace panels never
    /// resolve a pick/event target in this Unity version (IPanel.Pick and panel.visualTree.
    /// SendEvent both silently fail), while a panel left in its normal render mode with only
    /// PanelSettings.targetTexture assigned picks and dispatches correctly. VRUIPointerBridge
    /// raycasts the quad's MeshCollider (for RaycastHit.textureCoord) to turn an XR controller ray
    /// into panel-local pointer events. The original PanelSettings assets remain untouched, so the
    /// same prefabs still work on desktop.
    /// </summary>
    public sealed class VRUIToolkitPresenter : MonoBehaviour
    {
        [SerializeField] private Vector2 _referenceSize = new Vector2(1600f, 900f);
        [SerializeField] private float _distanceFromHead = 1.25f;
        [SerializeField] private float _worldScale = .001f;
        [SerializeField] private Vector3 _localOffset = new Vector3(0f, -.05f, 0f);

        private readonly List<PanelSettings> _runtimePanelSettings = new List<PanelSettings>();
        private readonly List<RenderTexture> _runtimeTextures = new List<RenderTexture>();
        private readonly List<Material> _runtimeMaterials = new List<Material>();
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

                RenderTexture renderTexture = new RenderTexture((int)_referenceSize.x, (int)_referenceSize.y, 0)
                {
                    name = document.name + "_VR_RT"
                };
                renderTexture.Create();
                _runtimeTextures.Add(renderTexture);

                PanelSettings runtimeSettings = Instantiate(document.panelSettings);
                runtimeSettings.name = document.panelSettings.name + "_VR_Runtime";
                runtimeSettings.targetTexture = renderTexture;
                _runtimePanelSettings.Add(runtimeSettings);
                document.panelSettings = runtimeSettings;

                document.transform.SetParent(_head, false);
                document.transform.localPosition = _localOffset + Vector3.forward * _distanceFromHead;
                document.transform.localRotation = Quaternion.identity;
                document.transform.localScale = Vector3.one;

                GameObject quadGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quadGo.name = "VRQuad";
                Destroy(quadGo.GetComponent<Collider>());
                Transform quad = quadGo.transform;
                quad.SetParent(document.transform, false);
                quad.localPosition = Vector3.zero;
                quad.localRotation = Quaternion.identity;
                quad.localScale = new Vector3(_referenceSize.x, _referenceSize.y, 1f) * _worldScale;

                Material material = new Material(Shader.Find("Unlit/Texture")) { mainTexture = renderTexture };
                _runtimeMaterials.Add(material);
                quadGo.GetComponent<MeshRenderer>().sharedMaterial = material;

                MeshCollider meshCollider = quadGo.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = quadGo.GetComponent<MeshFilter>().sharedMesh;

                VRUIPanelColliderController inputGate = quadGo.GetComponent<VRUIPanelColliderController>();
                if (inputGate == null) inputGate = quadGo.AddComponent<VRUIPanelColliderController>();
                bool isMenu = document.GetComponent<IMenuPanel>() != null;
                inputGate.Configure(isMenu
                    ? VRUIPanelColliderController.PanelKind.Menu
                    : VRUIPanelColliderController.PanelKind.Gameplay);

                VRUIPointerBridge bridge = quadGo.GetComponent<VRUIPointerBridge>();
                if (bridge == null) bridge = quadGo.AddComponent<VRUIPointerBridge>();
                bridge.Configure(document);

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

            foreach (RenderTexture texture in _runtimeTextures)
                if (texture != null) { texture.Release(); Destroy(texture); }
            _runtimeTextures.Clear();

            foreach (Material material in _runtimeMaterials)
                if (material != null) Destroy(material);
            _runtimeMaterials.Clear();
        }
    }
}
