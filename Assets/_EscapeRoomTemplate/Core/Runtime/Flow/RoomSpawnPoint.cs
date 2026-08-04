using UnityEngine;

namespace EscapeRoomRevolt.Core.Flow
{
    /// <summary>Marks where the player should appear after a room-to-room transition targets this id.</summary>
    public sealed class RoomSpawnPoint : MonoBehaviour
    {
        [SerializeField] private string _spawnId = "default";

        public string SpawnId => _spawnId;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, .35f);
            Gizmos.DrawRay(transform.position, transform.forward * .75f);
        }
    }
}
