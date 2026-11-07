using EscapeRoomRevolt.Systems.Interaction;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Equipment
{
    /// <summary>
    /// Keeps world interaction and equipped presentation on the same prefab.
    /// Gameplay behaviours stay on the root while the visual model remains replaceable.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public sealed class EquippableItem : InteractableBase
    {
        [Header("Equipment identity")]
        [SerializeField] private string _equipmentId = "equipment";

        [Header("Pose in EquipmentSocket")]
        [SerializeField] private Vector3 _equippedLocalPosition;
        [SerializeField] private Vector3 _equippedLocalEulerAngles;
        [SerializeField] private Vector3 _equippedLocalScale = Vector3.one;

        private Rigidbody _rigidbody;
        private Collider[] _colliders;
        private bool _originalUseGravity;
        private bool _originalIsKinematic;
        private CollisionDetectionMode _originalCollisionMode;

        public string EquipmentId => _equipmentId;
        public bool IsEquipped { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            _rigidbody = GetComponent<Rigidbody>();
            _colliders = GetComponentsInChildren<Collider>(true);
            _originalUseGravity = _rigidbody.useGravity;
            _originalIsKinematic = _rigidbody.isKinematic;
            _originalCollisionMode = _rigidbody.collisionDetectionMode;
        }

        protected override void OnInteract()
        {
            if (EquipmentController.Instance != null)
            {
                EquipmentController.Instance.TryEquip(this);
                return;
            }

            Debug.LogWarning("[EquippableItem] No EquipmentController found on the player.", this);
        }

        internal void AttachTo(Transform socket)
        {
            if (socket == null || IsEquipped) return;

            OnFocusExit();
            IsEquipped = true;
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.useGravity = false;
            _rigidbody.isKinematic = true;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;

            foreach (Collider itemCollider in _colliders)
                if (itemCollider != null) itemCollider.enabled = false;

            transform.SetParent(socket, false);
            transform.localPosition = _equippedLocalPosition;
            transform.localRotation = Quaternion.Euler(_equippedLocalEulerAngles);
            transform.localScale = _equippedLocalScale;
            NotifyLifecycle(equipped: true);
        }

        internal void DetachToWorld(Vector3 position, Quaternion rotation, Vector3 impulse)
        {
            if (!IsEquipped) return;

            NotifyLifecycle(equipped: false);
            IsEquipped = false;
            transform.SetParent(null, true);
            transform.SetPositionAndRotation(position, rotation);
            transform.localScale = Vector3.one;

            foreach (Collider itemCollider in _colliders)
                if (itemCollider != null) itemCollider.enabled = true;

            _rigidbody.isKinematic = _originalIsKinematic;
            _rigidbody.useGravity = _originalUseGravity;
            _rigidbody.collisionDetectionMode = _originalCollisionMode;
            if (!_rigidbody.isKinematic) _rigidbody.AddForce(impulse, ForceMode.VelocityChange);
        }

        private void NotifyLifecycle(bool equipped)
        {
            MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is not IEquipmentLifecycle lifecycle) continue;
                if (equipped) lifecycle.OnEquipped();
                else lifecycle.OnUnequipped();
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (string.IsNullOrWhiteSpace(_equipmentId)) _equipmentId = name;
            if (_equippedLocalScale == Vector3.zero) _equippedLocalScale = Vector3.one;
        }
#endif
    }
}
