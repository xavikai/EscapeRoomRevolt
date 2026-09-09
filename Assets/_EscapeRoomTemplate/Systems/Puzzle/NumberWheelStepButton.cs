using System.Collections;
using EscapeRoomRevolt.Systems.Interaction;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Puzzle
{
    /// <summary>A physical arrow button that changes one number wheel through its shared step API.</summary>
    public sealed class NumberWheelStepButton : InteractableBase
    {
        [SerializeField] private NumberWheelInteractable _wheel;
        [SerializeField] private int _direction = 1;
        [SerializeField, Min(0f)] private float _pressDepth = .025f;
        [SerializeField, Min(.1f)] private float _pressSpeed = 12f;

        private Vector3 _restPosition;
        private bool _isPressing;

        public override bool CanInteract => base.CanInteract && _wheel != null && _wheel.CanUseStepButtons;
        public override string InteractionPrompt => _direction > 0 ? "Pujar número" : "Baixar número";

        protected override void Awake()
        {
            base.Awake();
            _restPosition = transform.localPosition;
        }

        public void Configure(NumberWheelInteractable wheel, int direction)
        {
            _wheel = wheel;
            _direction = direction >= 0 ? 1 : -1;
        }

        protected override void OnInteract()
        {
            if (!_wheel.TryStep(_direction)) return;
            if (!_isPressing) StartCoroutine(PressAnimation());
        }

        private IEnumerator PressAnimation()
        {
            _isPressing = true;
            Vector3 pressedPosition = _restPosition + Vector3.forward * _pressDepth;

            while (Vector3.Distance(transform.localPosition, pressedPosition) > .001f)
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, pressedPosition,
                    Time.deltaTime * _pressSpeed);
                yield return null;
            }

            while (Vector3.Distance(transform.localPosition, _restPosition) > .001f)
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, _restPosition,
                    Time.deltaTime * _pressSpeed);
                yield return null;
            }

            transform.localPosition = _restPosition;
            _isPressing = false;
        }
    }
}
