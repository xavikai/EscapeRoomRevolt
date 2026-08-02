using System;
using System.Collections;
using EscapeRoomRevolt.Core;
using EscapeRoomRevolt.Core.Input;
using EscapeRoomRevolt.Core.Settings;
using EscapeRoomRevolt.Player;
using EscapeRoomRevolt.Systems.Inventory;
using EscapeRoomRevolt.Systems.Survival;
using UnityEngine;
using UnityEngine.AI;

namespace EscapeRoomRevolt.Systems.Interaction
{
    public enum DoorMovementType
    {
        Pivot,
        Slide
    }

    public enum DoorOperationMode
    {
        Normal,
        Careful,
        Slam
    }

    public enum DoorOpenStage
    {
        Closed,
        Peek,
        Open
    }

    /// <summary>
    /// Lockable, saveable door/container with procedural or Animator movement. Survival Horror can
    /// additionally use careful staged opening and loud slams without changing existing door prefabs.
    /// </summary>
    public class Door : InteractableBase, IInventoryItemTarget
    {
        [Header("Door Settings")]
        [SerializeField] private bool _isLocked;
        [SerializeField] private string _requiredItemId = "";
        [SerializeField] private string _lockedPrompt = "Locked";
        [SerializeField] private string _openPrompt = "Open";
        [SerializeField] private string _closePrompt = "Close";
        [SerializeField] private string _peekPrompt = "Open further";
        [SerializeField] private ItemUsePolicy _itemUsePolicy = ItemUsePolicy.OfferCompatible;
        [SerializeField] private bool _consumeRequiredItem = true;

        [Header("Advanced Operations")]
        [SerializeField] private bool _enableAdvancedOperations = true;
        [SerializeField, Range(.05f, .45f)] private float _peekFraction = .18f;
        [Tooltip("Seconds required to move through the complete range in careful mode.")]
        [SerializeField, Min(.1f)] private float _carefulDuration = 2.4f;
        [SerializeField, Range(.05f, .6f)] private float _slamDuration = .18f;
        [SerializeField, Range(.5f, .95f)] private float _navigationOpenThreshold = .72f;

        [Header("Enemy Interaction")]
        [SerializeField] private bool _allowEnemyOperation = true;
        [SerializeField] private bool _enemyCanBreakLock;

        [Header("Animation")]
        [SerializeField] private Animator _animator;
        [SerializeField] private string _openTrigger = "Open";
        [SerializeField] private string _closeTrigger = "Close";
        [SerializeField] private string _peekTrigger = "Peek";
        [SerializeField] private string _slamOpenTrigger = "SlamOpen";
        [SerializeField] private string _slamCloseTrigger = "SlamClose";

        [Header("Movement Settings")]
        [SerializeField] private DoorMovementType _movementType = DoorMovementType.Pivot;
        [Tooltip("Time in seconds to smoothly open the door in normal mode.")]
        [SerializeField, Min(.05f)] private float _openDuration = .8f;

        [Header("Pivot Settings (Rotation)")]
        [Tooltip("The object whose position defines the hinge. If null, the center of this object is used.")]
        [SerializeField] private Transform _customPivot;
        [SerializeField] private float _openAngle = 90f;
        [Tooltip("If true, the door swings away from the actor when opening from closed.")]
        [SerializeField] private bool _openAwayFromPlayer = true;

        [Header("Slide Settings (Translation)")]
        [SerializeField] private Vector3 _slideOffset = new Vector3(1.5f, 0f, 0f);

        [Header("Audio Settings")]
        [SerializeField] private AudioClip _openSound;
        [SerializeField] private AudioClip _closeSound;
        [SerializeField] private AudioClip _carefulSound;
        [SerializeField] private AudioClip _slamSound;
        [SerializeField] private AudioClip _lockedSound;
        [SerializeField] private float _pitchVariance = .1f;

        [Header("Gameplay Noise")]
        [SerializeField, Min(0f)] private float _carefulNoiseRadius = 2.5f;
        [SerializeField, Min(0f)] private float _normalOpenNoiseRadius = 8f;
        [SerializeField, Min(0f)] private float _normalCloseNoiseRadius = 11f;
        [SerializeField, Min(0f)] private float _slamNoiseRadius = 18f;

        private bool _isOpen;
        private bool _isMoving;
        private Coroutine _movementCoroutine;
        private Quaternion _closedRotation;
        private Vector3 _closedLocalPosition;
        private int _openDirection = 1;
        private Vector3 _worldHingePoint;
        private Vector3 _localPivotOffset;
        private NavMeshObstacle[] _navMeshObstacles;
        private float _openAmount;
        private float _targetOpenAmount;
        private DoorOperationMode _lastOperation = DoorOperationMode.Normal;
        private float _lastNoiseRadius;

        public override string InteractionPrompt
        {
            get
            {
                if (_isLocked) return _lockedPrompt;
                if (_isMoving) return _targetOpenAmount > _openAmount ? "Opening..." : "Closing...";
                if (_openAmount >= .99f) return _closePrompt;
                if (_openAmount > .01f && AdvancedOperationsEnabled) return _peekPrompt;
                return _openPrompt;
            }
        }

        public bool IsLocked => _isLocked;
        public bool IsOpen => _isOpen;
        public bool IsMoving => _isMoving;
        public bool IsAjar => _openAmount > .01f && _openAmount < .99f;
        public float OpenAmount => _openAmount;
        public float TargetOpenAmount => _targetOpenAmount;
        public DoorOpenStage OpenStage => _openAmount <= .01f
            ? DoorOpenStage.Closed
            : _openAmount >= .99f ? DoorOpenStage.Open : DoorOpenStage.Peek;
        public DoorOperationMode LastOperation => _lastOperation;
        public float LastNoiseRadius => _lastNoiseRadius;
        public bool AdvancedOperationsEnabled => _enableAdvancedOperations
            && GameFeatures.IsEnabled(OptionalGameFeature.AdvancedDoors);
        public string RequiredItemId => _requiredItemId;

        public event Action<DoorOperationMode, float> OperationStarted;
        public event Action<DoorOperationMode, float> OperationCompleted;

        protected override void Start()
        {
            base.Start();
            _navMeshObstacles = GetComponentsInChildren<NavMeshObstacle>(true);
            _closedRotation = transform.rotation;
            _closedLocalPosition = transform.localPosition;
            _worldHingePoint = _customPivot != null ? _customPivot.position : transform.position;
            _localPivotOffset = _customPivot != null
                ? transform.InverseTransformPoint(_customPivot.position)
                : Vector3.zero;
            _openAmount = _isOpen ? 1f : 0f;
            _targetOpenAmount = _openAmount;
            SetNavigationBlocked(_openAmount < _navigationOpenThreshold);
        }

        protected override void OnDestroy()
        {
            if (_movementCoroutine != null) StopCoroutine(_movementCoroutine);
            base.OnDestroy();
        }

        protected override void OnInteract()
        {
            if (!TryResolvePlayerLock()) return;

            DoorOperationMode mode = DoorOperationMode.Normal;
            if (AdvancedOperationsEnabled)
            {
                InputRouter input = InputRouter.Instance;
                if (input != null && input.CarefulInteractModifierHeld) mode = DoorOperationMode.Careful;
                else if (input != null && input.SprintHeld) mode = DoorOperationMode.Slam;
            }
            Operate(mode, PlayerPlatformRegistry.Current?.Head);
        }

        /// <summary>Operates the next logical stage. Existing callers can continue using Interact().</summary>
        public bool Operate(DoorOperationMode mode, Transform actor = null)
        {
            if (_isLocked) return false;
            if (!AdvancedOperationsEnabled) mode = DoorOperationMode.Normal;
            float target = ResolveNextTarget(mode);
            return OperateTo(target, mode, actor);
        }

        public void Unlock()
        {
            if (!_isLocked) return;
            _isLocked = false;
            EventBus.Publish(new OnLockStateChanged { lockableId = SaveId, isLocked = false });
        }

        public void Lock()
        {
            _isLocked = true;
            EventBus.Publish(new OnLockStateChanged { lockableId = SaveId, isLocked = true });
        }

        public void ForceOpen()
        {
            if (_isLocked) Unlock();
            OperateTo(1f, DoorOperationMode.Normal, PlayerPlatformRegistry.Current?.Head);
        }

        public void ForceClose()
        {
            OperateTo(0f, DoorOperationMode.Normal, PlayerPlatformRegistry.Current?.Head);
        }

        /// <summary>Backward-compatible AI entry point.</summary>
        public bool TryOpenForAI(bool forceLocked) => TryOpenForAI(forceLocked, DoorOperationMode.Normal);

        /// <summary>AI can use normal or slam operation. Locked doors require both force flags.</summary>
        public bool TryOpenForAI(bool forceLocked, DoorOperationMode mode)
        {
            if (_targetOpenAmount >= .99f) return true;
            if (!_allowEnemyOperation) return false;
            if (_isLocked)
            {
                if (!forceLocked || !_enemyCanBreakLock) return false;
                Unlock();
                GameplayNoise.Emit(transform.position, 15f, GameplayNoiseType.Impact, gameObject);
            }
            if (mode == DoorOperationMode.Careful) mode = DoorOperationMode.Normal;
            return OperateTo(1f, mode, null);
        }

        private bool TryResolvePlayerLock()
        {
            if (!_isLocked) return true;
            InventoryManager inventory = InventoryManager.Instance;
            ItemUseResult result = inventory != null
                ? inventory.RequestUseOnTarget(this)
                : ItemUseResult.NoCompatibleItem;
            if (result == ItemUseResult.Used) return true;
            if (result == ItemUseResult.OfferedSelection) return false;

            Debug.Log($"[Door] {name} is locked. Required item: {_requiredItemId}");
            PlaySound(_lockedSound);
            return false;
        }

        private float ResolveNextTarget(DoorOperationMode mode)
        {
            if (mode == DoorOperationMode.Careful)
            {
                if (_openAmount <= .01f && _targetOpenAmount <= .01f) return _peekFraction;
                if (_openAmount < .99f || _targetOpenAmount < .99f) return 1f;
                return 0f;
            }
            if (mode == DoorOperationMode.Slam)
                return Mathf.Max(_openAmount, _targetOpenAmount) > .01f ? 0f : 1f;
            return Mathf.Max(_openAmount, _targetOpenAmount) >= .5f ? 0f : 1f;
        }

        private bool OperateTo(float target, DoorOperationMode mode, Transform actor)
        {
            target = Mathf.Clamp01(target);
            if (Mathf.Abs(target - _openAmount) < .001f && !_isMoving) return true;

            if (target > _openAmount && _openAmount <= .01f)
                ResolveOpenDirection(actor);
            if (_movementCoroutine != null)
            {
                StopCoroutine(_movementCoroutine);
                _movementCoroutine = null;
            }

            _targetOpenAmount = target;
            _isOpen = target >= .99f;
            _lastOperation = mode;
            _isMoving = true;
            EmitOperationFeedback(mode, target > _openAmount);
            OperationStarted?.Invoke(mode, target);

            if (_animator != null)
            {
                TriggerAnimator(mode, target);
                _openAmount = target;
                _isMoving = false;
                SetNavigationBlocked(_openAmount < _navigationOpenThreshold);
                OperationCompleted?.Invoke(mode, target);
                return true;
            }

            float fullDuration = mode switch
            {
                DoorOperationMode.Careful => _carefulDuration,
                DoorOperationMode.Slam => _slamDuration,
                _ => _openDuration
            };
            float duration = Mathf.Max(.05f, fullDuration * Mathf.Abs(target - _openAmount));
            _movementCoroutine = StartCoroutine(MoveToAmount(_openAmount, target, duration, mode));
            return true;
        }

        private IEnumerator MoveToAmount(float start, float target, float duration, DoorOperationMode mode)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float raw = Mathf.Clamp01(elapsed / duration);
                float eased = raw * raw * (3f - 2f * raw);
                _openAmount = Mathf.Lerp(start, target, eased);
                ApplyOpenAmount(_openAmount);
                yield return null;
            }

            _openAmount = target;
            ApplyOpenAmount(target);
            _isMoving = false;
            _movementCoroutine = null;
            OperationCompleted?.Invoke(mode, target);
        }

        private void ApplyOpenAmount(float amount)
        {
            if (_movementType == DoorMovementType.Pivot)
            {
                Quaternion rotation = _closedRotation * Quaternion.Euler(0f, _openAngle * _openDirection * amount, 0f);
                transform.rotation = rotation;
                transform.position = _worldHingePoint - (rotation * _localPivotOffset);
            }
            else
            {
                transform.localPosition = _closedLocalPosition + _slideOffset * amount;
            }
            SetNavigationBlocked(amount < _navigationOpenThreshold);
        }

        private void ResolveOpenDirection(Transform actor)
        {
            if (_movementType != DoorMovementType.Pivot || !_openAwayFromPlayer || actor == null) return;
            Vector3 directionToActor = (actor.position - transform.position).normalized;
            _openDirection = Vector3.Dot(transform.forward, directionToActor) > 0f ? -1 : 1;
        }

        private void TriggerAnimator(DoorOperationMode mode, float target)
        {
            string trigger;
            if (mode == DoorOperationMode.Slam)
                trigger = target > .01f ? _slamOpenTrigger : _slamCloseTrigger;
            else if (mode == DoorOperationMode.Careful && target < .99f)
                trigger = _peekTrigger;
            else
                trigger = target > .01f ? _openTrigger : _closeTrigger;

            if (string.IsNullOrWhiteSpace(trigger))
                trigger = target > .01f ? _openTrigger : _closeTrigger;
            if (!string.IsNullOrWhiteSpace(trigger)) _animator.SetTrigger(trigger);
        }

        private void EmitOperationFeedback(DoorOperationMode mode, bool opening)
        {
            AudioClip clip = mode switch
            {
                DoorOperationMode.Careful => _carefulSound != null ? _carefulSound : opening ? _openSound : _closeSound,
                DoorOperationMode.Slam => _slamSound != null ? _slamSound : opening ? _openSound : _closeSound,
                _ => opening ? _openSound : _closeSound
            };
            PlaySound(clip);

            GameplayNoiseType noiseType;
            if (mode == DoorOperationMode.Careful)
            {
                noiseType = GameplayNoiseType.DoorCareful;
                _lastNoiseRadius = _carefulNoiseRadius;
            }
            else if (mode == DoorOperationMode.Slam)
            {
                noiseType = GameplayNoiseType.DoorSlam;
                _lastNoiseRadius = _slamNoiseRadius;
            }
            else
            {
                noiseType = GameplayNoiseType.Door;
                _lastNoiseRadius = opening ? _normalOpenNoiseRadius : _normalCloseNoiseRadius;
            }
            GameplayNoise.Emit(transform.position, _lastNoiseRadius, noiseType, gameObject);
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null && EscapeRoomRevolt.Systems.Audio.AudioManager.Instance != null)
                EscapeRoomRevolt.Systems.Audio.AudioManager.Instance.PlaySoundAt(
                    clip, transform.position, 1f, _pitchVariance);
        }

        private void SetNavigationBlocked(bool blocked)
        {
            if (_navMeshObstacles == null)
                _navMeshObstacles = GetComponentsInChildren<NavMeshObstacle>(true);
            foreach (NavMeshObstacle obstacle in _navMeshObstacles)
                if (obstacle != null) obstacle.enabled = blocked;
        }

        public ItemUsePolicy UsePolicy => _itemUsePolicy;
        public bool ConsumeItemOnUse => _consumeRequiredItem;
        public bool AcceptsItem(InventoryItemData item) => item != null
            && !string.IsNullOrWhiteSpace(_requiredItemId)
            && item.ItemId == _requiredItemId;

        public bool TryUseItem(InventoryItemData item)
        {
            if (!_isLocked || !AcceptsItem(item)) return false;
            Unlock();
            return true;
        }

        [Serializable]
        private sealed class DoorSaveState
        {
            public int version = 2;
            public bool isLocked;
            public bool isOpen;
            public float openAmount;
            public int openDirection = 1;
        }

        public override string SaveData()
        {
            return JsonUtility.ToJson(new DoorSaveState
            {
                isLocked = _isLocked,
                isOpen = _isOpen,
                openAmount = _openAmount,
                openDirection = _openDirection
            });
        }

        public override void LoadData(string json)
        {
            DoorSaveState state = JsonUtility.FromJson<DoorSaveState>(json);
            if (state == null) return;
            if (_movementCoroutine != null)
            {
                StopCoroutine(_movementCoroutine);
                _movementCoroutine = null;
            }

            _isLocked = state.isLocked;
            _openAmount = state.version >= 2 ? Mathf.Clamp01(state.openAmount) : state.isOpen ? 1f : 0f;
            _targetOpenAmount = _openAmount;
            _isOpen = _openAmount >= .99f;
            _isMoving = false;
            _openDirection = state.openDirection == 0 ? 1 : Math.Sign(state.openDirection);

            if (_animator != null)
                TriggerAnimator(DoorOperationMode.Normal, _openAmount);
            else
                ApplyOpenAmount(_openAmount);
            SetNavigationBlocked(_openAmount < _navigationOpenThreshold);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _peekFraction = Mathf.Clamp(_peekFraction, .05f, .45f);
            _carefulDuration = Mathf.Max(.1f, _carefulDuration);
            _slamDuration = Mathf.Max(.05f, _slamDuration);
            _openDuration = Mathf.Max(.05f, _openDuration);
        }
#endif
    }
}
