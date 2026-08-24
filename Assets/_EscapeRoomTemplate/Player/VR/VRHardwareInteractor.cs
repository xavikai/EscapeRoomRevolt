using EscapeRoomRevolt.Systems.Interaction;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace EscapeRoomRevolt.Player.VR
{
    /// <summary>
    /// Quest-safe interaction path for world objects. It reads the physical controller buttons
    /// directly and raycasts IInteractable components, so gameplay remains usable even when an
    /// XRI/UI event module is unavailable or a headset runtime maps Select differently.
    /// </summary>
    public sealed class VRHardwareInteractor : MonoBehaviour
    {
        [Tooltip("Maximum reach for VR world interactions. Kept short so mechanisms require the player to approach them.")]
        [SerializeField, Min(.5f)] private float _maxDistance = 2.25f;
        [SerializeField] private LayerMask _interactionMask = ~0;
        [SerializeField] private float _rayWidth = .006f;
        [SerializeField] private Color _idleColor = new Color(.25f, .65f, 1f, .75f);
        [SerializeField] private Color _hoverColor = new Color(1f, .75f, .1f, 1f);
        [Header("Throwing")]
        [SerializeField, Min(0f)] private float _throwVelocityScale = 1.65f;
        [SerializeField, Min(0f)] private float _throwAngularVelocityScale = 1f;
        [SerializeField, Range(.02f, .25f)] private float _throwSmoothingTime = .09f;
        [SerializeField, Min(1f)] private float _maximumThrowSpeed = 12f;

        private VRPlayerPlatformAdapter _adapter;
        private HandState _left;
        private HandState _right;

        private sealed class HandState
        {
            public XRNode node;
            public PlayerHand hand;
            public Transform origin;
            public LineRenderer line;
            public Material material;
            public IInteractable focused;
            public bool wasTriggerPressed;
            public bool wasGrabPressed;
            public MonoBehaviour held;
            public Rigidbody heldBody;
            public Transform originalParent;
            public bool originalKinematic;
            public bool originalGravity;
            public bool hasPoseSample;
            public Vector3 previousOriginPosition;
            public Quaternion previousOriginRotation;
            public Vector3 smoothedLinearVelocity;
            public Vector3 smoothedAngularVelocity;
        }

        private void Awake()
        {
            _adapter = GetComponent<VRPlayerPlatformAdapter>();
            _left = CreateHand(XRNode.LeftHand, PlayerHand.Left, _adapter?.GetHand(PlayerHand.Left));
            _right = CreateHand(XRNode.RightHand, PlayerHand.Right, _adapter?.GetHand(PlayerHand.Right));
        }

        private void Update()
        {
            if (_adapter == null) _adapter = GetComponent<VRPlayerPlatformAdapter>();
            if (_left == null || _left.origin == null)
                _left = ReconnectHand(_left, XRNode.LeftHand, PlayerHand.Left);
            if (_right == null || _right.origin == null)
                _right = ReconnectHand(_right, XRNode.RightHand, PlayerHand.Right);
            Process(_left);
            Process(_right);
        }

        private HandState ReconnectHand(HandState previous, XRNode node, PlayerHand hand)
        {
            Transform controller = _adapter != null ? _adapter.GetHand(hand) : null;
            if (controller == null) return previous ?? new HandState { node = node, hand = hand };

            if (previous?.line != null) Destroy(previous.line.gameObject);
            if (previous?.material != null) Destroy(previous.material);
            HandState connected = CreateHand(node, hand, controller);
            Debug.Log($"[VR Hardware] Reconnected {hand} controller ray to '{controller.name}'.");
            return connected;
        }

        private HandState CreateHand(XRNode node, PlayerHand hand, Transform controller)
        {
            if (controller == null) return new HandState { node = node, hand = hand };

            NearFarInteractor nearFar = controller.GetComponentInChildren<NearFarInteractor>(true);
            Transform origin = nearFar != null && nearFar.curveOrigin != null ? nearFar.curveOrigin : controller;

            GameObject rayObject = new GameObject($"{hand} Hardware Interaction Ray");
            rayObject.transform.SetParent(transform, false);
            LineRenderer line = rayObject.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.startWidth = _rayWidth;
            line.endWidth = _rayWidth * .35f;
            line.numCapVertices = 4;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            Material material = shader != null ? new Material(shader) : null;
            if (material != null)
            {
                material.color = _idleColor;
                line.sharedMaterial = material;
            }

            return new HandState { node = node, hand = hand, origin = origin, line = line, material = material };
        }

        private void Process(HandState state)
        {
            if (state == null || state.origin == null || state.line == null) return;
            SampleHandVelocity(state);

            Ray ray = new Ray(state.origin.position, state.origin.forward);
            float distance = _maxDistance;
            IInteractable target = null;
            if (Physics.Raycast(ray, out RaycastHit hit, _maxDistance, _interactionMask, QueryTriggerInteraction.Collide))
            {
                distance = hit.distance;
                target = ResolveInteractable(hit.collider);
            }

            if (!ReferenceEquals(target, state.focused))
            {
                if (state.focused.IsAlive()) state.focused.OnFocusExit();
                state.focused = target;
                if (state.focused.IsAlive()) state.focused.OnFocusEnter();
            }

            state.line.SetPosition(0, ray.origin);
            state.line.SetPosition(1, ray.GetPoint(distance));
            if (state.material != null) state.material.color = target.IsAlive() ? _hoverColor : _idleColor;

            InputDevice device = InputDevices.GetDeviceAtXRNode(state.node);
            bool grip = device.isValid && device.TryGetFeatureValue(CommonUsages.gripButton, out bool gripValue) && gripValue;
            bool trigger = device.isValid && device.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerValue) && triggerValue;
            MonoBehaviour grabbable = target as MonoBehaviour;
            bool targetIsGrabbable = grabbable != null && grabbable.GetType().Name == "PhysicsGrabbable";
            bool grabPressed = grip || (trigger && (targetIsGrabbable || state.held != null));

            // Quest users naturally try both the side grip and the index trigger. World props use
            // hold-to-grab semantics with either button; releasing the last held button drops them.
            if (grabPressed && !state.wasGrabPressed && targetIsGrabbable)
                BeginGrab(state, grabbable);
            else if (!grabPressed && state.wasGrabPressed && state.held != null)
                EndGrab(state);

            if (trigger && !state.wasTriggerPressed && target.IsAlive() && state.held == null && !targetIsGrabbable)
            {
                bool performed = InteractionDispatcher.TryPerform(target, state.hand);
                if (performed && target is MonoBehaviour behaviour)
                    Debug.Log($"[VR Hardware] {state.hand} interacted with '{behaviour.name}'.");
            }

            state.wasTriggerPressed = trigger;
            state.wasGrabPressed = grabPressed;
        }

        private void BeginGrab(HandState state, MonoBehaviour grabbable)
        {
            Rigidbody body = grabbable != null ? grabbable.GetComponent<Rigidbody>() : null;
            if (body == null || state.origin == null) return;

            state.held = grabbable;
            state.heldBody = body;
            state.originalParent = grabbable.transform.parent;
            state.originalKinematic = body.isKinematic;
            state.originalGravity = body.useGravity;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.isKinematic = true;
            body.useGravity = false;
            grabbable.transform.SetParent(state.origin, true);
            grabbable.transform.localPosition = new Vector3(0f, 0f, .18f);
            grabbable.transform.localRotation = Quaternion.identity;
            _adapter?.SendHaptic(state.hand, .35f, .06f);
            Debug.Log($"[VR Hardware] {state.hand} grabbed '{grabbable.name}'.");
        }

        private void EndGrab(HandState state)
        {
            if (state?.held == null || state.heldBody == null) return;

            MonoBehaviour released = state.held;
            Rigidbody body = state.heldBody;
            // This player assembly deliberately has no direct dependency on the gameplay assembly.
            // Read the public flag on PhysicsGrabbable only when the object is released.
            var canBeThrownProperty = released.GetType().GetProperty("CanBeThrown");
            bool canBeThrown = canBeThrownProperty?.PropertyType == typeof(bool)
                && (bool)canBeThrownProperty.GetValue(released);
            released.transform.SetParent(state.originalParent, true);
            body.isKinematic = state.originalKinematic;
            body.useGravity = state.originalGravity;
            if (!body.isKinematic)
            {
                if (canBeThrown)
                {
                    body.linearVelocity = Vector3.ClampMagnitude(
                        state.smoothedLinearVelocity * _throwVelocityScale, _maximumThrowSpeed);
                    body.angularVelocity = Vector3.ClampMagnitude(
                        state.smoothedAngularVelocity * _throwAngularVelocityScale, _maximumThrowSpeed * 2f);
                    if (body.linearVelocity.sqrMagnitude >= .09f)
                        released.SendMessage("OnThrown", SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    body.linearVelocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                }
            }
            released.SendMessage("OnDropped", SendMessageOptions.DontRequireReceiver);
            Debug.Log($"[VR Hardware] released '{released.name}'.");
            state.held = null;
            state.heldBody = null;
            state.originalParent = null;
        }

        private void SampleHandVelocity(HandState state)
        {
            float deltaTime = Time.unscaledDeltaTime;
            Vector3 position = state.origin.position;
            Quaternion rotation = state.origin.rotation;
            if (!state.hasPoseSample || deltaTime <= Mathf.Epsilon)
            {
                state.hasPoseSample = true;
                state.previousOriginPosition = position;
                state.previousOriginRotation = rotation;
                state.smoothedLinearVelocity = Vector3.zero;
                state.smoothedAngularVelocity = Vector3.zero;
                return;
            }

            Vector3 linearVelocity = (position - state.previousOriginPosition) / deltaTime;
            Quaternion rotationDelta = rotation * Quaternion.Inverse(state.previousOriginRotation);
            rotationDelta.ToAngleAxis(out float angleDegrees, out Vector3 axis);
            if (angleDegrees > 180f) angleDegrees -= 360f;
            Vector3 angularVelocity = axis.sqrMagnitude > .0001f
                ? axis.normalized * (angleDegrees * Mathf.Deg2Rad / deltaTime)
                : Vector3.zero;

            float blend = 1f - Mathf.Exp(-deltaTime / Mathf.Max(.001f, _throwSmoothingTime));
            state.smoothedLinearVelocity = Vector3.Lerp(state.smoothedLinearVelocity, linearVelocity, blend);
            state.smoothedAngularVelocity = Vector3.Lerp(state.smoothedAngularVelocity, angularVelocity, blend);
            state.previousOriginPosition = position;
            state.previousOriginRotation = rotation;
        }

        private static IInteractable ResolveInteractable(Collider collider)
        {
            Transform cursor = collider != null ? collider.transform : null;
            while (cursor != null)
            {
                foreach (MonoBehaviour behaviour in cursor.GetComponents<MonoBehaviour>())
                    if (behaviour is IInteractable interactable && interactable.CanInteract) return interactable;
                cursor = cursor.parent;
            }
            return null;
        }

        private void OnDisable()
        {
            if (_left?.focused.IsAlive() == true) _left.focused.OnFocusExit();
            if (_right?.focused.IsAlive() == true) _right.focused.OnFocusExit();
            if (_left?.held != null) EndGrab(_left);
            if (_right?.held != null) EndGrab(_right);
        }

        private void OnDestroy()
        {
            if (_left?.material != null) Destroy(_left.material);
            if (_right?.material != null) Destroy(_right.material);
        }
    }
}
