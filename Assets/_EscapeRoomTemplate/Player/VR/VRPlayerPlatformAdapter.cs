using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using Unity.XR.CoreUtils;

namespace EscapeRoomRevolt.Player.VR
{
    /// <summary>XR-device bridge used by gameplay code without taking a dependency on a headset vendor SDK.</summary>
    public sealed class VRPlayerPlatformAdapter : PlayerPlatformAdapterBase
    {
        [SerializeField] private Transform _head;
        [SerializeField] private Transform _leftHand;
        [SerializeField] private Transform _rightHand;
        private XROrigin _origin;

        public override PlayerPlatform Platform => PlayerPlatform.VirtualReality;
        public override Transform Head => _head != null ? _head : transform;
        public override bool SupportsHaptics => true;
        public override Transform GetHand(PlayerHand hand) => hand == PlayerHand.Left ? _leftHand : _rightHand;

        private void Awake()
        {
            _origin = GetComponent<XROrigin>();
            if (_head == null)
            {
                Camera camera = GetComponentInChildren<Camera>(true);
                if (camera != null) _head = camera.transform;
            }
            if (_leftHand == null) _leftHand = FindHand(transform, "left");
            if (_rightHand == null) _rightHand = FindHand(transform, "right");
        }

        public void Configure(Transform head, Transform leftHand, Transform rightHand)
        {
            _head = head;
            _leftHand = leftHand;
            _rightHand = rightHand;
            _origin = GetComponent<XROrigin>();
        }

        /// <summary>Moves the tracking origin to a floor-level gameplay anchor without changing the user's physical head offset.</summary>
        public void TeleportRig(Vector3 position, Quaternion rotation)
        {
            if (_origin == null) _origin = GetComponent<XROrigin>();
            if (_origin == null)
            {
                transform.SetPositionAndRotation(position, rotation);
                return;
            }

            _origin.MatchOriginUpOriginForward(Vector3.up, rotation * Vector3.forward);
            Vector3 floor = _origin.Camera != null ? _origin.Camera.transform.position : _origin.transform.position;
            floor.y = _origin.transform.position.y;
            _origin.transform.position += position - floor;
        }

        public override void SendHaptic(PlayerHand hand, float amplitude, float duration)
        {
            XRNode node = hand == PlayerHand.Left ? XRNode.LeftHand : XRNode.RightHand;
            InputDevice device = InputDevices.GetDeviceAtXRNode(node);
            if (!device.isValid) return;
            device.SendHapticImpulse(0u, Mathf.Clamp01(amplitude), Mathf.Max(0f, duration));
            if (duration > 0f) StartCoroutine(StopHaptic(device, duration));
        }

        private static IEnumerator StopHaptic(InputDevice device, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            if (device.isValid) device.StopHaptics();
        }

        private static Transform FindHand(Transform root, string side)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                string lower = child.name.ToLowerInvariant();
                if (lower.Contains(side) && (lower.Contains("hand") || lower.Contains("controller"))) return child;
            }
            return null;
        }
    }
}
