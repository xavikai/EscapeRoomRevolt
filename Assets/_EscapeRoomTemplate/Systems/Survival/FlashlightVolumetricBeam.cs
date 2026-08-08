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
        [Tooltip("Distance from the bulb where the visible beam starts. Keeping this off zero avoids the near end filling the whole screen when looking straight down the beam (first person).")]
        [SerializeField, Range(.1f, 1f)] private float _nearDistance = .35f;
        [SerializeField, Min(3)] private int _segments = 20;
        [SerializeField, Range(0f, 2f)] private float _coreIntensity = .35f;
        [SerializeField, Range(0f, 2f)] private float _outerIntensity = .12f;

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

            float near = Mathf.Min(_nearDistance, length * .5f);
            _coreRenderer = BuildFrustum("VolumetricCore", near, length, _light.innerSpotAngle * .5f, _coreMaterial);
            _outerRenderer = BuildFrustum("VolumetricOuter", near, length, _light.spotAngle * .5f, _outerMaterial);

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
        /// Builds an open frustum (no near or far cap, opening along local +Z) whose vertex alpha
        /// fades from brightest at the near ring to 0 at the far ring. Starting the near ring away
        /// from the bulb - instead of a true apex at the origin - keeps the beam from filling the
        /// whole screen when the camera looks straight down its own beam (first person).
        /// </summary>
        private MeshRenderer BuildFrustum(string goName, float nearDistance, float farDistance, float halfAngleDegrees, Material material)
        {
            float nearRadius = nearDistance * Mathf.Tan(halfAngleDegrees * Mathf.Deg2Rad);
            float farRadius = farDistance * Mathf.Tan(halfAngleDegrees * Mathf.Deg2Rad);

            var vertices = new Vector3[_segments * 2];
            var normals = new Vector3[_segments * 2];
            var colors = new Color[_segments * 2];
            var triangles = new int[_segments * 6];

            for (int i = 0; i < _segments; i++)
            {
                float t = i / (float)_segments * Mathf.PI * 2f;
                float cos = Mathf.Cos(t);
                float sin = Mathf.Sin(t);
                Vector3 radial = new Vector3(cos, sin, 0f);
                vertices[i] = new Vector3(cos * nearRadius, sin * nearRadius, nearDistance);
                normals[i] = radial;
                colors[i] = new Color(1f, 1f, 1f, 1f);
                vertices[_segments + i] = new Vector3(cos * farRadius, sin * farRadius, farDistance);
                normals[_segments + i] = radial;
                colors[_segments + i] = new Color(1f, 1f, 1f, 0f);
            }
            for (int i = 0; i < _segments; i++)
            {
                int nearA = i;
                int nearB = (i + 1) % _segments;
                int farA = _segments + i;
                int farB = _segments + (i + 1) % _segments;

                triangles[i * 6] = nearA;
                triangles[i * 6 + 1] = farA;
                triangles[i * 6 + 2] = nearB;

                triangles[i * 6 + 3] = nearB;
                triangles[i * 6 + 4] = farA;
                triangles[i * 6 + 5] = farB;
            }

            var mesh = new Mesh { name = goName };
            mesh.vertices = vertices;
            mesh.normals = normals;
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
