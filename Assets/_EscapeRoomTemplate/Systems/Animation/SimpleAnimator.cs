using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace EscapeRoomRevolt.Systems.Animation
{
    /// <summary>
    /// A simple utility to animate an object's position or rotation over time
    /// the moment it spawns. Perfect for keys turning, drawers opening, etc.
    /// </summary>
    public class SimpleAnimator : MonoBehaviour
    {
        [Header("Timing")]
        [Tooltip("Time to wait before starting the animation (in seconds)")]
        [SerializeField] private float _delayBeforeStart = 0f;
        
        [Tooltip("How long the animation takes to complete")]
        [SerializeField] private float _duration = 1f;

        [Header("Animation Settings")]
        [Tooltip("How much to rotate locally (e.g., X:0, Y:90, Z:0 to turn a key)")]
        [SerializeField] private Vector3 _rotationAmount;
        
        [Tooltip("How much to move locally (e.g., X:0, Y:0, Z:0.5 to open a drawer)")]
        [SerializeField] private Vector3 _movementAmount;

        [Header("Trigger")]
        [Tooltip("If true, the animation starts automatically when the object spawns or the game starts.")]
        [SerializeField] private bool _playOnAwake = true;
        
        [Header("Events")]
        [Tooltip("Triggered exactly when the animation finishes.")]
        public UnityEvent OnAnimationFinished;

        private void Start()
        {
            if (_playOnAwake && enabled)
            {
                PlayAnimation();
            }
        }

        /// <summary>
        /// Immediately snaps the object to its final animated state and stops further animation.
        /// Useful when loading a game where the animation already happened.
        /// </summary>
        public void ForceToEndState()
        {
            StopAllCoroutines();
            
            transform.localPosition = transform.localPosition + _movementAmount;
            transform.localRotation = transform.localRotation * Quaternion.Euler(_rotationAmount);
            
            enabled = false;
        }

        /// <summary>
        /// Call this from a UnityEvent to start the animation manually.
        /// </summary>
        public void PlayAnimation()
        {
            StartCoroutine(AnimateRoutine());
        }

        private IEnumerator AnimateRoutine()
        {
            if (_delayBeforeStart > 0f)
            {
                yield return new WaitForSeconds(_delayBeforeStart);
            }

            Vector3 startPos = transform.localPosition;
            Vector3 targetPos = startPos + _movementAmount;

            Quaternion startRot = transform.localRotation;
            Quaternion targetRot = startRot * Quaternion.Euler(_rotationAmount);

            float elapsed = 0f;

            // Prevent division by zero if duration is 0
            if (_duration <= 0f)
            {
                transform.localPosition = targetPos;
                transform.localRotation = targetRot;
            }
            else
            {
                while (elapsed < _duration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / _duration);
                    
                    // Smoothstep easing formula for a natural acceleration/deceleration
                    t = t * t * (3f - 2f * t);

                    transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
                    transform.localRotation = Quaternion.Slerp(startRot, targetRot, t);

                    yield return null;
                }
            }

            // Ensure final exact values
            transform.localPosition = targetPos;
            transform.localRotation = targetRot;

            // Trigger anything that happens AFTER the animation (e.g., door opens!)
            OnAnimationFinished?.Invoke();
        }
    }
}
