using System;
using EscapeRoomRevolt.Core.Input;
using EscapeRoomRevolt.Core.Save;
using EscapeRoomRevolt.Systems.Interaction;
using UnityEngine;
using EscapeRoomRevolt.Player;

namespace EscapeRoomRevolt.Systems.Equipment
{
    [Serializable]
    public sealed class EquipmentSaveState
    {
        public string equippedItemId;
    }

    /// <summary>Owns the player's hand socket and the currently equipped world item.</summary>
    [DefaultExecutionOrder(-20)]
    public sealed class EquipmentController : MonoBehaviour, ISaveable
    {
        public static EquipmentController Instance { get; private set; }

        [SerializeField] private Transform _equipmentSocket;
        [Tooltip("Optional. When assigned (VR only — Setup wires this to the left ModelSocket), equipping with the left hand attaches here instead of the primary socket above.")]
        [SerializeField] private Transform _leftEquipmentSocket;
        [SerializeField] private float _dropDistance = .65f;
        [SerializeField] private float _dropImpulse = 1.25f;

        public EquippableItem CurrentItem { get; private set; }
        public bool HasEquippedItem => CurrentItem != null;
        public Transform EquipmentSocket => _equipmentSocket;
        public event Action<EquippableItem> EquipmentChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            ResolveSocket();
            SaveManager.Instance?.Register(this);
        }

        private void Update()
        {
            if (CurrentItem == null || IsGameplayBlocked()) return;

            if (InputRouter.Instance != null && InputRouter.Instance.DropEquippedPressed) DropCurrent();
        }

        private void OnDestroy()
        {
            SaveManager.Instance?.Unregister(this);
            if (Instance == this) Instance = null;
        }

        public bool TryEquip(EquippableItem item)
        {
            Transform targetSocket = InteractionDispatcher.LastHand == PlayerHand.Left && _leftEquipmentSocket != null
                ? _leftEquipmentSocket
                : _equipmentSocket;
            if (item == null || item == CurrentItem || targetSocket == null) return false;

            if (EscapeRoomRevolt.Systems.Interaction.PhysicsGrabber.Instance != null
                && EscapeRoomRevolt.Systems.Interaction.PhysicsGrabber.Instance.IsHoldingObject)
                EscapeRoomRevolt.Systems.Interaction.PhysicsGrabber.Instance.Drop();

            if (CurrentItem != null) DropCurrent();

            CurrentItem = item;
            CurrentItem.AttachTo(targetSocket);
            EquipmentChanged?.Invoke(CurrentItem);
            return true;
        }

        public void DropCurrent()
        {
            if (CurrentItem == null) return;

            Transform origin = PlayerPlatformRegistry.Current?.Head;
            if (origin == null)
            {
                Camera playerCamera = GetComponentInChildren<Camera>(true);
                origin = playerCamera != null ? playerCamera.transform : transform;
            }
            Vector3 position = origin.position + origin.forward * _dropDistance - origin.up * .2f;
            Quaternion rotation = origin.rotation;
            Vector3 impulse = origin.forward * _dropImpulse;

            EquippableItem droppedItem = CurrentItem;
            CurrentItem = null;
            droppedItem.DetachToWorld(position, rotation, impulse);
            EquipmentChanged?.Invoke(null);
        }

        private void ResolveSocket()
        {
            if (_equipmentSocket != null) return;

            Transform platformHand = PlayerPlatformRegistry.Current?.GetHand(PlayerHand.Right);
            if (platformHand != null)
            {
                _equipmentSocket = platformHand;
                return;
            }

            Camera playerCamera = GetComponentInChildren<Camera>(true);
            if (playerCamera == null)
            {
                Debug.LogError("[EquipmentController] Player camera not found; equipment cannot be shown.", this);
                return;
            }

            _equipmentSocket = playerCamera.transform.Find("EquipmentSocket");
            if (_equipmentSocket != null) return;

            var socketObject = new GameObject("EquipmentSocket");
            _equipmentSocket = socketObject.transform;
            _equipmentSocket.SetParent(playerCamera.transform, false);
            _equipmentSocket.localPosition = new Vector3(.28f, -.24f, .55f);
        }

        private static bool IsGameplayBlocked()
        {
            return (EscapeRoomRevolt.UI.PC.UIManager.Instance != null
                    && EscapeRoomRevolt.UI.PC.UIManager.Instance.IsUIBlockingGameplay)
                || (EscapeRoomRevolt.UI.Toolkit.UIToolkitMenuController.Instance != null
                    && EscapeRoomRevolt.UI.Toolkit.UIToolkitMenuController.Instance.IsBlockingGameplay);
        }

        public string SaveId => "EquipmentController";

        public string SaveData() => JsonUtility.ToJson(new EquipmentSaveState
        {
            equippedItemId = CurrentItem != null ? CurrentItem.EquipmentId : string.Empty
        });

        public void LoadData(string json)
        {
            var state = JsonUtility.FromJson<EquipmentSaveState>(json);
            if (state == null) return;
            if (CurrentItem != null) DropCurrent();
            if (string.IsNullOrEmpty(state.equippedItemId)) return;

            EquippableItem[] items = FindObjectsByType<EquippableItem>(FindObjectsInactive.Include);
            foreach (EquippableItem item in items)
            {
                if (item.EquipmentId != state.equippedItemId) continue;
                TryEquip(item);
                break;
            }
        }
    }
}
