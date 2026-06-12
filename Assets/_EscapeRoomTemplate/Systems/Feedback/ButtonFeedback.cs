using System.Collections;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Feedback
{
    public class ButtonFeedback : MonoBehaviour
    {
        [Tooltip("The visual object to push inward.")]
        [SerializeField] private Transform _visualTransform;
        [Tooltip("How far inward it goes when pushed.")]
        [SerializeField] private float _pushDistance = 0.05f;
        [Tooltip("Duration of the push animation.")]
        [SerializeField] private float _pushDuration = 0.1f;
        [Tooltip("Audio to play when pushed.")]
        [SerializeField] private AudioSource _audioSource;

        private Vector3 _originalPosition;
        private Coroutine _pushCoroutine;

        private void Start()
        {
            if (_visualTransform != null)
            {
                _originalPosition = _visualTransform.localPosition;
            }
        }

        public void PlayFeedback()
        {
            if (_audioSource != null)
            {
                _audioSource.Play();
            }

            if (_visualTransform != null)
            {
                if (_pushCoroutine != null)
                {
                    StopCoroutine(_pushCoroutine);
                    _visualTransform.localPosition = _originalPosition;
                }
                _pushCoroutine = StartCoroutine(PushRoutine());
            }
        }

        private IEnumerator PushRoutine()
        {
            Vector3 targetPosition = _originalPosition + Vector3.forward * _pushDistance;
            
            float elapsed = 0f;
            while (elapsed < _pushDuration / 2f)
            {
                elapsed += Time.deltaTime;
                _visualTransform.localPosition = Vector3.Lerp(_originalPosition, targetPosition, elapsed / (_pushDuration / 2f));
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < _pushDuration / 2f)
            {
                elapsed += Time.deltaTime;
                _visualTransform.localPosition = Vector3.Lerp(targetPosition, _originalPosition, elapsed / (_pushDuration / 2f));
                yield return null;
            }
            
            _visualTransform.localPosition = _originalPosition;
        }
    }
}
