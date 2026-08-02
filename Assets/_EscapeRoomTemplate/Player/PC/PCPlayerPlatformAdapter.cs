using UnityEngine;

namespace EscapeRoomRevolt.Player.PC
{
    public sealed class PCPlayerPlatformAdapter : PlayerPlatformAdapterBase
    {
        [SerializeField] private Transform _head;
        [SerializeField] private Transform _handSocket;

        public override PlayerPlatform Platform => PlayerPlatform.Desktop;
        public override Transform Head => _head != null ? _head : transform;
        public override bool SupportsHaptics => false;
        public override Transform GetHand(PlayerHand hand) => _handSocket != null ? _handSocket : Head;
        public override void SendHaptic(PlayerHand hand, float amplitude, float duration) { }

        private void Awake()
        {
            if (_head == null)
            {
                Camera camera = GetComponentInChildren<Camera>(true);
                if (camera != null) _head = camera.transform;
            }
            if (_handSocket == null && _head != null) _handSocket = _head.Find("EquipmentSocket");
        }
    }
}
