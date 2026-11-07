using UnityEngine;

namespace EscapeRoomRevolt.Systems.Interaction
{
    /// <summary>Accepts body colliders on a tagged player or any of its children.</summary>
    public static class PlayerTriggerUtility
    {
        public static bool IsPlayer(Collider other)
        {
            if (other == null) return false;
            for (Transform current = other.transform; current != null; current = current.parent)
                if (current.CompareTag("Player")) return true;
            return false;
        }
    }
}
