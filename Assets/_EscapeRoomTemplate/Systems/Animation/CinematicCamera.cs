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
        private Coroutine _routine;
        private float _originalDepth;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
            _originalDepth = _cam.depth;
            // Keep the behaviour active so UnityEvents can always reach it; only disable rendering.
            _cam.enabled = false;
        }

        /// <summary>
        /// Call this from a UnityEvent (like OnItemAccepted) to show the camera.
        /// </summary>
        public void PlayCinematic()
        {
            if (!gameObject.activeInHierarchy)
            {
                Debug.LogWarning($"[CinematicCamera] '{name}' is under an inactive parent and cannot play.", this);
                return;
            }

            if (_routine != null)
                StopCoroutine(_routine);

            _routine = StartCoroutine(CinematicRoutine());
        }

        private IEnumerator CinematicRoutine()
        {
            _cam.enabled = true;
            _cam.depth = 10;
            
            yield return new WaitForSecondsRealtime(_duration);
            
            _cam.enabled = false;
            _cam.depth = _originalDepth;
            _routine = null;
        }

        private void OnDisable()
        {
            if (_cam != null)
            {
                _cam.enabled = false;
                _cam.depth = _originalDepth;
            }

            _routine = null;
        }
    }
}
