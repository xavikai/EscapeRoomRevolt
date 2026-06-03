using System.Collections;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Animation
{
    /// <summary>
    /// Put this on a Camera to easily trigger cinematic shots from UnityEvents.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CinematicCamera : MonoBehaviour
    {
        [Tooltip("How many seconds the camera stays on before returning to the player")]
        [SerializeField] private float _duration = 2f;

        private Camera _cam;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
            // Make sure it's disabled at the start of the game
            _cam.gameObject.SetActive(false);
        }

        /// <summary>
        /// Call this from a UnityEvent (like OnItemAccepted) to show the camera.
        /// </summary>
        public void PlayCinematic()
        {
            // Unity coroutines can only run if the object is active!
            gameObject.SetActive(true);

            if (!gameObject.activeInHierarchy)
            {
                Debug.LogWarning("[CinematicCamera] Cannot play cinematic because a parent object is disabled. Unparenting camera temporarely...");
                transform.SetParent(null);
                gameObject.SetActive(true);
            }

            StartCoroutine(CinematicRoutine());
        }

        private IEnumerator CinematicRoutine()
        {
            // Ensure the camera component itself is also active
            _cam.enabled = true;
            _cam.depth = 10; // Ensure it renders on top of the player camera
            
            yield return new WaitForSeconds(_duration);
            
            _cam.gameObject.SetActive(false);
        }
    }
}
