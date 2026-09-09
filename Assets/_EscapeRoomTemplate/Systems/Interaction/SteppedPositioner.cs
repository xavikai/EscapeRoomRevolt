using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace EscapeRoomRevolt.Systems.Interaction
{
    public enum SteppedMovementType
    {
        Rotate,
        Slide
    }

    [System.Serializable]
    public sealed class SteppedPosition
    {
        [Tooltip("Interaction prompt shown while this is the current position.")]
        public string prompt = "Interact";
        [Tooltip("Local euler angles for this position (used when Movement Type is Rotate).")]
        public Vector3 rotation = Vector3.zero;
        [Tooltip("Local position for this position (used when Movement Type is Slide).")]
        public Vector3 position = Vector3.zero;
    }

    /// <summary>
    /// Moves an object between N discrete rotation or slide positions, animating each transition.
    /// Deliberately not an interactable itself, so whatever drives it is free to vary: a player click
    /// (InteractableCycler), or a puzzle-specific bridge that owns the click for its own reasons
    /// (PipeTileButton). Objects that already carry another interactable can therefore reuse this
    /// movement without ending up with two competing IInteractables on the same GameObject.
    /// </summary>
    public class SteppedPositioner : MonoBehaviour
    {
        [Header("Positions")]
        [Tooltip("Every state this object can be in, in cycle order. Needs at least 2.")]
        [SerializeField] private List<SteppedPosition> _positions = new List<SteppedPosition>
        {
            new SteppedPosition(), new SteppedPosition()
        };
        [SerializeField] private int _startingIndex;

        [Header("Movement Settings")]
        [SerializeField] private SteppedMovementType _movementType = SteppedMovementType.Rotate;
        [SerializeField] private float _transitionDuration = 0.2f;
        [Tooltip("What actually moves. If null, resolves the '<name>_Visuals' child, falling back to this transform.")]
        [SerializeField] private Transform _visualTransform;

        [Header("Audio")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _stepSound;

        [Header("Events")]
        public UnityEvent<int> OnPositionChanged = new UnityEvent<int>();

        public int CurrentIndex { get; private set; }
        public int PositionCount => _positions.Count;
        public string CurrentPrompt => _positions.Count > 0 ? _positions[CurrentIndex].prompt : "Interact";

        private Coroutine _transitionCoroutine;

        private void Start()
        {
            if (_visualTransform == null)
            {
                _visualTransform = transform.Find(gameObject.name.Replace("_Logic", "") + "_Visuals");
                if (_visualTransform == null) _visualTransform = transform;
            }

            CurrentIndex = _positions.Count > 0 ? Mathf.Clamp(_startingIndex, 0, _positions.Count - 1) : 0;
            ApplyStateInstantly();
        }

        /// <summary>Advances to the next position (wrapping).</summary>
        public void Advance() => Step(1);

        /// <summary>Moves to the previous position (wrapping).</summary>
        public void Previous() => Step(-1);

        /// <summary>
        /// Moves in either direction through the authored positions. Positive values move forward,
        /// negative values move backward and both directions wrap around the ends.
        /// </summary>
        public void Step(int delta)
        {
            if (_positions.Count == 0 || delta == 0) return;
            int wrappedDelta = delta % _positions.Count;
            CurrentIndex = (CurrentIndex + wrappedDelta + _positions.Count) % _positions.Count;

            if (_audioSource != null && _stepSound != null) _audioSource.PlayOneShot(_stepSound);

            if (_transitionCoroutine != null) StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = StartCoroutine(TransitionRoutine());

            OnPositionChanged?.Invoke(CurrentIndex);
        }

        /// <summary>Jumps straight to a position without animating or firing OnPositionChanged — for syncing to externally-owned state (a loaded save, a randomized starting rotation).</summary>
        public void SetIndexInstant(int index)
        {
            if (_positions.Count == 0) return;
            CurrentIndex = Mathf.Clamp(index, 0, _positions.Count - 1);
            if (_visualTransform == null) _visualTransform = transform;
            ApplyStateInstantly();
        }

        private void ApplyStateInstantly()
        {
            if (_positions.Count == 0 || _visualTransform == null) return;
            SteppedPosition target = _positions[CurrentIndex];
            if (_movementType == SteppedMovementType.Rotate)
                _visualTransform.localRotation = Quaternion.Euler(target.rotation);
            else
                _visualTransform.localPosition = target.position;
        }

        private IEnumerator TransitionRoutine()
        {
            if (_visualTransform == null) yield break;

            float elapsed = 0f;
            SteppedPosition target = _positions[CurrentIndex];

            Quaternion startRot = _visualTransform.localRotation;
            Quaternion endRot = Quaternion.Euler(target.rotation);
            Vector3 startPos = _visualTransform.localPosition;
            Vector3 endPos = target.position;

            while (elapsed < _transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _transitionDuration;

                if (_movementType == SteppedMovementType.Rotate)
                    _visualTransform.localRotation = Quaternion.Slerp(startRot, endRot, t);
                else
                    _visualTransform.localPosition = Vector3.Lerp(startPos, endPos, t);

                yield return null;
            }

            ApplyStateInstantly();
        }
    }
}
