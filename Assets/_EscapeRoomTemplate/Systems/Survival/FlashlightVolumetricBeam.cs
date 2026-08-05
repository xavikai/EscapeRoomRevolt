using UnityEngine;
using UnityEngine.Rendering;

namespace EscapeRoomRevolt.Systems.Survival
{
    /// <summary>
    /// Fakes a volumetric light shaft for the flashlight using two overlapping soft-additive cone
    /// meshes (a narrow bright core plus a wider dim glow), built once from the Light's own spot
    /// angle/range/color. This project's installed URP version has no native volumetric fog
    /// support, so this is the standard Forward-renderer workaround instead of a real fog volume.
    /// Add alongside the Light on the flashlight's LightSocket; no other wiring required.
    /// </summary>
    [RequireComponent(typeof(Light))]
    public sealed class FlashlightVolumetricBeam : MonoBehaviour
    {
        [SerializeField] private FlashlightController _flashlight;
        [SerializeField, Range(.1f, 1f)] private float _lengthScale = .8f;
        [SerializeField, Min(3)] private int _segments = 20;
        [SerializeField, Range(0f, 5f)] private float _coreIntensity = 1.6f;
        [SerializeField, Range(0f, 5f)] private float _outerIntensity = .5f;

        private Light _light;
        private MeshRenderer _coreRenderer;
        private MeshRenderer _outerRenderer;
        private Material _coreMaterial;
        private Material _outerMaterial;

        private void Awake()
        {
            _light = GetComponent<Light>();
            if (_flashlight == null) _flashlight = GetComponentInParent<FlashlightController>();

            Shader shader = Shader.Find("EscapeRoom/FlashlightVolumetricCone");
            if (shader == null) { enabled = false; return; }

            float length = Mathf.Max(.1f, _light.range * _lengthScale);
            _coreMaterial = BuildMaterial(shader, _light.color, _coreIntensity);
            _outerMaterial = BuildMaterial(shader, _light.color, _outerIntensity);

            _coreRenderer = BuildCone("VolumetricCore", length, _light.innerSpotAngle * .5f, _coreMaterial);
            _outerRenderer = BuildCone("VolumetricOuter", length, _light.spotAngle * .5f, _outerMaterial);

            ApplyVisibility();
        }

        private void OnEnable()
        {
            if (_flashlight != null) _flashlight.StateChanged += ApplyVisibility;
        }

        private void OnDisable()
        {
            if (_flashlight != null) _flashlight.StateChanged -= ApplyVisibility;
        }

        private void OnDestroy()
        {
            if (_coreMaterial != null) Destroy(_coreMaterial);
            if (_outerMaterial != null) Destroy(_outerMaterial);
        }

        private void ApplyVisibility()
        {
            bool on = _flashlight != null && _flashlight.IsOn;
            if (_coreRenderer != null) _coreRenderer.enabled = on;
            if (_outerRenderer != null) _outerRenderer.enabled = on;
        }

        private static Material BuildMaterial(Shader shader, Color color, float intensity)
        {
            var material = new Material(shader) { hideFlags = HideFlags.DontSave };
            material.SetColor("_Color", color);
            material.SetFloat("_Intensity", intensity);
            return material;
        }

        /// <summary>
        /// Builds a cone (apex at the local origin, opening along local +Z) whose vertex alpha
        /// fades from 1 at the apex to 0 at the outer ring, so the additive shader reads as a
        /// beam that is brightest near the bulb and dissolves toward the far edge.
        /// </summary>
        private MeshRenderer BuildCone(string goName, float length, float halfAngleDegrees, Material material)
        {
            float radius = length * Mathf.Tan(halfAngleDegrees * Mathf.Deg2Rad);

            var vertices = new Vector3[_segments + 1];
            var colors = new Color[_segments + 1];
            var triangles = new int[_segments * 3];

            vertices[0] = Vector3.zero;
            colors[0] = new Color(1f, 1f, 1f, 1f);
            for (int i = 0; i < _segments; i++)
            {
                float t = i / (float)_segments * Mathf.PI * 2f;
                vertices[i + 1] = new Vector3(Mathf.Cos(t) * radius, Mathf.Sin(t) * radius, length);
                colors[i + 1] = new Color(1f, 1f, 1f, 0f);
            }
            for (int i = 0; i < _segments; i++)
            {
                int current = i + 1;
                int next = (i + 1) % _segments + 1;
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = current;
                triangles[i * 3 + 2] = next;
            }

            var mesh = new Mesh { name = goName };
            mesh.vertices = vertices;
            mesh.colors = colors;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();

            var go = new GameObject(goName);
            go.transform.SetParent(transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            return renderer;
        }
    }
}
