using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using EscapeRoomRevolt.Core;

namespace EscapeRoomRevolt.Systems.Interaction
{
    public enum ToggleMovementType
    {
        Rotate,
        Slide
    }

    /// <summary>
    /// An interactable object with two states (On/Off).
    /// Can visually rotate (like a lever) or slide (like a switch).
    /// </summary>
    public class InteractableToggle : InteractableBase
    {
        [Header("Toggle Settings")]
        [SerializeField] private bool _isOn = false;
        [SerializeField] private string _promptOn = "Turn Off";
        [SerializeField] private string _promptOff = "Turn On";

        [Header("Movement Settings")]
        [SerializeField] private ToggleMovementType _movementType = ToggleMovementType.Rotate;
        [SerializeField] private float _transitionDuration = 0.2f;
        [SerializeField] private Transform _visualTransform;

        [Header("Rotate Settings")]
        [Tooltip("The object whose position defines the hinge. If null, the visualTransform's own center is used.")]
        [SerializeField] private Transform _customPivot;
        [SerializeField] private Vector3 _offAngles = Vector3.zero;
        [SerializeField] private Vector3 _onAngles = new Vector3(45f, 0f, 0f);

        [Header("Slide Settings")]
        [SerializeField] private Vector3 _offPosition = Vector3.zero;
        [SerializeField] private Vector3 _onPosition = new Vector3(0f, -0.1f, 0f);

        [Header("Audio")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _toggleSound;

        [Header("Events")]
        public UnityEvent<bool> OnStateToggled;
        
        public bool IsOn => _isOn;
        
        public override string InteractionPrompt => _isOn ? _promptOn : _promptOff;

        private Coroutine _transitionCoroutine;

        protected override void Start()
        {
            base.Start();
            
            if (_visualTransform == null)
            {
                _visualTransform = transform.Find(gameObject.name.Replace("_Logic", "") + "_Visuals");
                if (_visualTransform == null) _visualTransform = transform;
            }

            // Set initial state instantly
            ApplyStateInstantly();
        }

        protected override void OnInteract()
        {
            _isOn = !_isOn;
            
            if (_audioSource != null && _toggleSound != null)
            {
                _audioSource.PlayOneShot(_toggleSound);
            }

            if (_transitionCoroutine != null) StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = StartCoroutine(TransitionRoutine());

            OnStateToggled?.Invoke(_isOn);
        }

        private void ApplyStateInstantly()
        {
            if (_movementType == ToggleMovementType.Rotate)
            {
                if (_customPivot != null)
                {
                    _visualTransform.RotateAround(_customPivot.position, _customPivot.right, _isOn ? _onAngles.x : _offAngles.x);
                    // Note: A simpler approach is just to make _customPivot the _visualTransform itself.
                    // For full robustness we just rotate the transform directly.
                    _visualTransform.localRotation = Quaternion.Euler(_isOn ? _onAngles : _offAngles);
                }
                else
                {
                    _visualTransform.localRotation = Quaternion.Euler(_isOn ? _onAngles : _offAngles);
                }
            }
            else
            {
                _visualTransform.localPosition = _isOn ? _onPosition : _offPosition;
            }
        }

        private IEnumerator TransitionRoutine()
        {
            float elapsed = 0f;
            
            Quaternion startRot = _visualTransform.localRotation;
            Quaternion endRot = Quaternion.Euler(_isOn ? _onAngles : _offAngles);

            Vector3 startPos = _visualTransform.localPosition;
            Vector3 endPos = _isOn ? _onPosition : _offPosition;

            Vector3 pivotPos = _customPivot != null ? _customPivot.position : _visualTransform.position;

            while (elapsed < _transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _transitionDuration;

                if (_movementType == ToggleMovementType.Rotate)
                {
                    _visualTransform.localRotation = Quaternion.Slerp(startRot, endRot, t);
                }
                else
                {
                    _visualTransform.localPosition = Vector3.Lerp(startPos, endPos, t);
                }

                yield return null;
            }

            ApplyStateInstantly();
        }
    }
}
