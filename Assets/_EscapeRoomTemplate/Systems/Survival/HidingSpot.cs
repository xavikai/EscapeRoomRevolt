using System;
using EscapeRoomRevolt.Core.Input;
using EscapeRoomRevolt.Core.Settings;
using EscapeRoomRevolt.Player;
using EscapeRoomRevolt.Player.PC;
using EscapeRoomRevolt.Player.VR;
using EscapeRoomRevolt.Systems.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace EscapeRoomRevolt.Systems.Survival
{
    public enum HidingSpotKind { Locker, UnderBed, Container, Custom }

    /// <summary>Reusable locker/bed/closet hiding interaction. Visuals live under ModelSocket.</summary>
    [RequireComponent(typeof(Collider))]
    public sealed class HidingSpot : InteractableBase
    {
        public static HidingSpot ActiveForPlayer { get; private set; }

        [SerializeField] private Transform _insideAnchor;
        [SerializeField] private Transform _exitAnchor;
        [SerializeField] private Transform _inspectionAnchor;
        [SerializeField] private HidingSpotKind _kind = HidingSpotKind.Locker;
        [SerializeField] private bool _forceCrouchedPose;
        [SerializeField] private string _enterPrompt = "Amagar-se";
        [SerializeField] private string _exitPrompt = "Sortir";
        [SerializeField, Min(0f)] private float _minimumStayTime = .25f;
        [SerializeField, Min(0f)] private float _exposureDamage = 30f;
        [Header("Breathing signal")]
        [SerializeField, Range(0f, 1f)] private float _calmBreathingIntensity = .2f;
        [SerializeField, Range(0f, 1f)] private float _chaseBreathingIntensity = 1f;
        [SerializeField, Min(.01f)] private float _breathingResponse = 2.5f;
        [Header("Events")]
        [SerializeField] private UnityEvent _onEntered;
        [SerializeField] private UnityEvent _onExited;
        [SerializeField] private UnityEvent _onForcedExpose;
        private Transform _playerRoot;
        private PlayerMovement _pcMovement;
        private VRComfortController _vrComfort;
        private PlayerVitals _vitals;
        private float _enteredAt;
        private float _breathingIntensity;

        public override string InteractionPrompt => ActiveForPlayer == this ? _exitPrompt : _enterPrompt;
        public override bool CanInteract => base.CanInteract && GameFeatures.IsEnabled(OptionalGameFeature.Hiding) && ActiveForPlayer == null;
        public bool IsOccupied => ActiveForPlayer == this;
        public Vector3 InspectionPosition => _inspectionAnchor != null ? _inspectionAnchor.position : transform.position;
        public HidingSpotKind Kind => _kind;
        public float BreathingIntensity => _breathingIntensity;
        public event Action<HidingSpot> Entered;
        public event Action<HidingSpot> Exited;
        public event Action<HidingSpot> ForcedExpose;
        public event Action<float> BreathingIntensityChanged;

        protected override void Awake()
        {
            base.Awake();
            if (!GameFeatures.IsEnabled(OptionalGameFeature.Hiding)) gameObject.SetActive(false);
        }

        private void Update()
        {
            if (ActiveForPlayer != this) return;
            float target = ChaseDirector.Instance != null && ChaseDirector.Instance.IsChaseActive
                ? _chaseBreathingIntensity
                : _calmBreathingIntensity;
            float next = Mathf.MoveTowards(_breathingIntensity, target, _breathingResponse * Time.deltaTime);
            if (!Mathf.Approximately(next, _breathingIntensity))
            {
                _breathingIntensity = next;
                BreathingIntensityChanged?.Invoke(next);
            }
            if (Time.time <= _enteredAt + _minimumStayTime) return;
            if (InputRouter.Instance != null && InputRouter.Instance.InteractPressed) ExitImmediately();
        }

        protected override void OnInteract()
        {
            if (ActiveForPlayer == this) ExitImmediately(); else Enter();
        }

        private void Enter()
        {
            PlayerPlatformAdapterBase adapter = PlayerPlatformRegistry.Current as PlayerPlatformAdapterBase;
            _playerRoot = adapter != null ? adapter.transform : null;
            if (_playerRoot == null)
            {
                _pcMovement = FindAnyObjectByType<PlayerMovement>();
                _playerRoot = _pcMovement != null ? _pcMovement.transform : null;
            }
            if (_playerRoot == null) return;
            _pcMovement = _playerRoot.GetComponent<PlayerMovement>();
            _vrComfort = _playerRoot.GetComponent<VRComfortController>();
            _vitals = _playerRoot.GetComponent<PlayerVitals>();
            ActiveForPlayer = this;
            _enteredAt = Time.time;
            if (_forceCrouchedPose) _pcMovement?.SetForcedCrouch(true);
            MovePlayer(_insideAnchor != null ? _insideAnchor : transform);
            if (_pcMovement != null) _pcMovement.IsMovementFrozen = true;
            _vrComfort?.SetMovementBlocked(true);
            _vitals?.SetHidden(true);
            _onEntered?.Invoke();
            Entered?.Invoke(this);
        }

        public void ExitImmediately()
        {
            if (ActiveForPlayer != this) return;
            MovePlayer(_exitAnchor != null ? _exitAnchor : transform);
            if (_forceCrouchedPose) _pcMovement?.SetForcedCrouch(false);
            if (_pcMovement != null) _pcMovement.IsMovementFrozen = false;
            _vrComfort?.SetMovementBlocked(false);
            _vitals?.SetHidden(false);
            SetBreathingIntensity(0f);
            _onExited?.Invoke();
            Exited?.Invoke(this);
            ActiveForPlayer = null;
            _playerRoot = null;
            _pcMovement = null;
            _vrComfort = null;
            _vitals = null;
        }

        public void ForceExpose()
        {
            if (ActiveForPlayer != this) return;
            PlayerVitals target = _vitals;
            ExitImmediately();
            _onForcedExpose?.Invoke();
            ForcedExpose?.Invoke(this);
            target?.ApplyDamage(new DamageInfo(_exposureDamage, DamageType.Enemy, gameObject, InspectionPosition));
        }

        private void SetBreathingIntensity(float value)
        {
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(_breathingIntensity, value)) return;
            _breathingIntensity = value;
            BreathingIntensityChanged?.Invoke(value);
        }

        private void MovePlayer(Transform anchor)
        {
            if (_playerRoot == null || anchor == null) return;
            VRPlayerPlatformAdapter vrAdapter = _playerRoot.GetComponent<VRPlayerPlatformAdapter>();
            if (vrAdapter != null)
            {
                vrAdapter.TeleportRig(anchor.position, anchor.rotation);
                return;
            }
            CharacterController controller = _playerRoot.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;
            _playerRoot.SetPositionAndRotation(anchor.position, anchor.rotation);
            if (controller != null) controller.enabled = true;
        }

        protected override void OnDestroy()
        {
            if (ActiveForPlayer == this) ExitImmediately();
            base.OnDestroy();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => ActiveForPlayer = null;
    }
}
