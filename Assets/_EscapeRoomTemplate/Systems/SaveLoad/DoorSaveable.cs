using UnityEngine;
using EscapeRoomRevolt.Systems.Interaction;

namespace EscapeRoomRevolt.Systems.SaveLoad
{
    /// <summary>
    /// Saves the locked and open state of a door.
    /// Attach this to the same GameObject as the Door script.
    /// </summary>
    [RequireComponent(typeof(Door))]
    public class DoorSaveable : MonoBehaviour, ISaveable
    {
        private Door _door;

        private void Awake()
        {
            _door = GetComponent<Door>();
        }

        public string SaveId => _door.SaveId;

        [System.Serializable]
        private class DoorSaveState
        {
            public bool isLocked;
            public bool isOpen;
        }

        public object SaveState()
        {
            return new DoorSaveState
            {
                isLocked = _door.IsLocked,
                isOpen = _door.IsOpen
            };
        }

        public void LoadState(object state)
        {
            if (state is string stateJson)
            {
                var loadedState = JsonUtility.FromJson<DoorSaveState>(stateJson);
                
                if (loadedState.isLocked != _door.IsLocked)
                {
                    if (loadedState.isLocked) _door.Lock();
                    else _door.Unlock();
                }

                // If the state is out of sync with the animation, we'd theoretically want to force 
                // the animator into the correct state instantly without playing the transition.
                if (loadedState.isOpen != _door.IsOpen)
                {
                    _door.Interact(); // Toggles the state
                }
            }
        }
    }
}
