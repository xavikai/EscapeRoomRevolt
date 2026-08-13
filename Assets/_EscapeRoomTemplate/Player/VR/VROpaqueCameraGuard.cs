using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;

namespace EscapeRoomRevolt.Player.VR
{
    /// <summary>
    /// Quest builds must submit an opaque eye texture for the escape-room scenes. A transparent
    /// camera background makes the headset compositor show passthrough instead of the game.
    /// </summary>
    public sealed class VROpaqueCameraGuard : MonoBehaviour
    {
        private readonly HashSet<int> _reportedDisabledCameras = new HashSet<int>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            GameObject guard = new GameObject("VR Opaque Camera Guard");
            DontDestroyOnLoad(guard);
            guard.AddComponent<VROpaqueCameraGuard>();
#endif
        }

        private void Awake()
        {
            Apply();
            Debug.Log("[VR Camera] Opaque rendering guard enabled.");
        }

        private void LateUpdate() => Apply();

        private void Apply()
        {
            XROrigin origin = FindAnyObjectByType<XROrigin>();
            Camera xrCamera = origin != null ? origin.Camera : null;
            VRPlayerPlatformAdapter adapter = FindAnyObjectByType<VRPlayerPlatformAdapter>();
            if (xrCamera == null && adapter != null && adapter.Head != null)
            {
                xrCamera = adapter.Head.GetComponent<Camera>();
                if (xrCamera == null) xrCamera = adapter.Head.GetComponentInChildren<Camera>(true);
                if (xrCamera == null) xrCamera = adapter.Head.GetComponentInParent<Camera>();
            }

            foreach (Camera camera in Camera.allCameras)
            {
                if (camera == null) continue;

                // PC puzzle-focus and cinematic cameras are valid on a monitor, but must never
                // become the headset's display camera. If one takes over, head tracking appears
                // to pivot around the wrong point and the tracked controller rays are out of view.
                if (xrCamera != null && camera != xrCamera && camera.targetTexture == null)
                {
                    camera.enabled = false;
                    AudioListener listener = camera.GetComponent<AudioListener>();
                    if (listener != null) listener.enabled = false;
                    if (_reportedDisabledCameras.Add(camera.GetInstanceID()))
                        Debug.LogWarning($"[VR Camera] Disabled non-XR display camera '{camera.name}'.");
                    continue;
                }

                Color background = camera.backgroundColor;
                background.a = 1f;
                camera.backgroundColor = background;
                camera.clearFlags = CameraClearFlags.SolidColor;
            }
        }
    }
}
