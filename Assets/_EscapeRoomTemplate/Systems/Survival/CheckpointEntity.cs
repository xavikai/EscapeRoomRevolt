using EscapeRoomRevolt.Core.Save;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Survival
{
    public readonly struct CheckpointEntityState
    {
        public readonly bool Active;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly Vector3 Scale;
        public readonly Vector3 Velocity;
        public readonly Vector3 AngularVelocity;

        public CheckpointEntityState(bool active, Transform target, Rigidbody body)
        {
            Active = active;
            Position = target.position;
            Rotation = target.rotation;
            Scale = target.localScale;
            Velocity = body != null ? body.linearVelocity : Vector3.zero;
            AngularVelocity = body != null ? body.angularVelocity : Vector3.zero;
        }
    }

    /// <summary>Keeps scene entities restorable without instantiating or destroying runtime clones.</summary>
    [DisallowMultipleComponent]
    public sealed class CheckpointEntity : MonoBehaviour
    {
        [SerializeField] private string _checkpointId;
        [SerializeField] private bool _restoreTransform = true;
        [SerializeField] private bool _restoreRigidbody = true;
        private Rigidbody _body;

        public string CheckpointId
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_checkpointId)) return _checkpointId;
                foreach (MonoBehaviour behaviour in GetComponents<MonoBehaviour>())
                    if (behaviour is ISaveable saveable && !string.IsNullOrWhiteSpace(saveable.SaveId))
                        return saveable.SaveId;
                return name;
            }
        }

        private void Awake() => _body = GetComponent<Rigidbody>();

        public CheckpointEntityState Capture() => new CheckpointEntityState(gameObject.activeSelf, transform, _body);

        public void RemoveFromWorld() => gameObject.SetActive(false);

        public void Restore(in CheckpointEntityState state)
        {
            if (_body == null) _body = GetComponent<Rigidbody>();
            if (_restoreTransform)
            {
                transform.SetPositionAndRotation(state.Position, state.Rotation);
                transform.localScale = state.Scale;
            }
            if (_restoreRigidbody && _body != null)
            {
                _body.linearVelocity = state.Velocity;
                _body.angularVelocity = state.AngularVelocity;
                _body.Sleep();
            }
            gameObject.SetActive(state.Active);
        }
    }
}
