using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

namespace EscapeRoomRevolt.Player.VR
{
    /// <summary>Applies the selected comfort profile to Unity's XRI locomotion providers.</summary>
    public sealed class VRComfortController : MonoBehaviour
    {
        [SerializeField] private VRComfortSettings _settings;
        private ContinuousMoveProvider _continuousMove;
        private TeleportationProvider _teleportation;
        private SnapTurnProvider _snapTurn;
        private ContinuousTurnProvider _continuousTurn;
        private bool _movementBlocked;

        public VRComfortSettings Settings => _settings;

        private void Awake()
        {
            if (_settings == null) _settings = Resources.Load<VRComfortSettings>("VRComfortSettings");
            ResolveProviders();
            Apply();
        }

        public void Apply()
        {
            ResolveProviders();
            if (_settings == null) return;

            bool continuous = !_movementBlocked && _settings.locomotionMode != VRLocomotionMode.TeleportOnly;
            bool teleport = !_movementBlocked && _settings.locomotionMode != VRLocomotionMode.ContinuousOnly;
            if (_continuousMove != null)
            {
                _continuousMove.moveSpeed = _settings.continuousMoveSpeed;
                _continuousMove.enabled = continuous;
            }
            if (_teleportation != null) _teleportation.enabled = teleport;
            if (_snapTurn != null)
            {
                _snapTurn.turnAmount = _settings.snapTurnAmount;
                _snapTurn.enabled = !_movementBlocked && _settings.turnMode == VRTurnMode.Snap;
            }
            if (_continuousTurn != null)
            {
                _continuousTurn.turnSpeed = _settings.continuousTurnSpeed;
                _continuousTurn.enabled = !_movementBlocked && _settings.turnMode == VRTurnMode.Continuous;
            }
        }

        public void SetMovementBlocked(bool blocked)
        {
            if (_movementBlocked == blocked) return;
            _movementBlocked = blocked;
            Apply();
        }

        private void ResolveProviders()
        {
            if (_continuousMove == null) _continuousMove = GetComponentInChildren<ContinuousMoveProvider>(true);
            if (_teleportation == null) _teleportation = GetComponentInChildren<TeleportationProvider>(true);
            if (_snapTurn == null) _snapTurn = GetComponentInChildren<SnapTurnProvider>(true);
            if (_continuousTurn == null) _continuousTurn = GetComponentInChildren<ContinuousTurnProvider>(true);
        }
    }
}
