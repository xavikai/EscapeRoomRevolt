using EscapeRoomRevolt.Core.Settings;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Survival
{
    [RequireComponent(typeof(Collider))]
    public sealed class SurvivalCheckpoint : MonoBehaviour
    {
        [SerializeField] private string _checkpointId = "checkpoint";
        [SerializeField] private bool _isInitial;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private bool _oneShot = true;
        private bool _reached;

        public string CheckpointId => string.IsNullOrWhiteSpace(_checkpointId) ? name : _checkpointId;
        public bool IsInitial => _isInitial;
        public Vector3 SpawnPosition => _spawnPoint != null ? _spawnPoint.position : transform.position;
        public Quaternion SpawnRotation => _spawnPoint != null ? _spawnPoint.rotation : transform.rotation;

        private void Awake()
        {
            if (!GameFeatures.IsEnabled(OptionalGameFeature.Checkpoints)) { gameObject.SetActive(false); return; }
            GetComponent<Collider>().isTrigger = true;
        }

        private void Start() => CheckpointManager.Instance?.Register(this);

        private void OnTriggerEnter(Collider other)
        {
            if ((_oneShot && _reached) || !other.CompareTag("Player")) return;
            _reached = true;
            CheckpointManager.Instance?.Reach(this);
        }
    }
}
