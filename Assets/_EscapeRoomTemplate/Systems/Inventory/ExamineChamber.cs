using UnityEngine;

namespace EscapeRoomRevolt.Systems.Inventory
{
    /// <summary>
    /// Put this script on a specific empty GameObject in your scene (e.g., at Y=1000).
    /// This is where 3D items will be instantiated to be examined.
    /// </summary>
    public class ExamineChamber : MonoBehaviour
    {
        public static ExamineChamber Instance { get; private set; }

        [Tooltip("The exact point where the 3D model will be spawned. Should be in front of the Examine Camera.")]
        [SerializeField] private Transform _spawnPoint;

        public Transform SpawnPoint => _spawnPoint != null ? _spawnPoint : transform;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }
    }
}
