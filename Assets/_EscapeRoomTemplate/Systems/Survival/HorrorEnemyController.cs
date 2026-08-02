using System;
using System.Collections;
using EscapeRoomRevolt.Core.Settings;
using EscapeRoomRevolt.Systems.Interaction;
using UnityEngine;
using UnityEngine.AI;

namespace EscapeRoomRevolt.Systems.Survival
{
    public enum HorrorEnemyState { Idle, Patrol, Suspicious, Investigate, Search, Chase, Return }

    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class HorrorEnemyController : MonoBehaviour
    {
        [SerializeField] private HorrorEnemyProfile _profile;
        [SerializeField] private Transform _eye;
        [SerializeField] private Transform _player;
        [SerializeField] private Transform[] _patrolPoints = Array.Empty<Transform>();
        [SerializeField] private LayerMask _visionBlockingMask = ~0;
        [SerializeField, Min(0f)] private float _waypointTolerance = .35f;
        [Header("Traversal")]
        [SerializeField, Min(.25f)] private float _traversalDetectionDistance = 1.75f;
        [SerializeField, Range(.05f, .5f)] private float _traversalDetectionRadius = .18f;
        [SerializeField, Min(.1f)] private float _traversalRetryDelay = .65f;

        private NavMeshAgent _agent;
        private PlayerVitals _vitals;
        private PlayerVisibility _visibility;
        private HorrorEnemyState _state;
        private Vector3 _spawnPosition;
        private Quaternion _spawnRotation;
        private Vector3 _lastKnownPosition;
        private float _lastSeenTime = float.NegativeInfinity;
        private float _searchUntil;
        private float _nextPerception;
        private float _nextAttack;
        private float _awareness;
        private float _detectionSuppressedUntil;
        private float _nextDoorInteraction;
        private float _inspectHidingAt;
        private HidingSpot _suspectedHidingSpot;
        private int _patrolIndex;
        private Coroutine _traversalRoutine;
        private TraversalObstacle _activeTraversal;
        private Vector3 _traversalSafePosition;
        private Quaternion _traversalSafeRotation;
        private float _nextTraversalAttempt;

        public HorrorEnemyState State => _state;
        public bool IsChasing => _state == HorrorEnemyState.Chase;
        public float Awareness => _awareness;
        public bool IsTraversing => _traversalRoutine != null;
        public TraversalObstacle ActiveTraversal => _activeTraversal;
        public event Action<HorrorEnemyState> StateChanged;
        public event Action<bool> ChaseChanged;
        public event Action<float> AwarenessChanged;

        private float PatrolSpeed => (_profile != null ? _profile.patrolSpeed : 1.8f) * SurvivalDifficultyService.EnemySpeed;
        private float InvestigateSpeed => (_profile != null ? _profile.investigateSpeed : 2.7f) * SurvivalDifficultyService.EnemySpeed;
        private float ChaseSpeed => (_profile != null ? _profile.chaseSpeed : 4.8f) * SurvivalDifficultyService.EnemySpeed;
        private float SightRange => (_profile != null ? _profile.sightRange : 14f) * SurvivalDifficultyService.EnemySight;
        private float SightAngle => _profile != null ? _profile.sightAngle : 85f;
        private float Hearing => (_profile != null ? _profile.hearingMultiplier : 1f) * SurvivalDifficultyService.EnemyHearing;
        private float PerceptionInterval => _profile != null ? _profile.perceptionInterval : .12f;
        private float DetectionSeconds => _profile != null ? _profile.detectionSeconds : .35f;
        private float AwarenessDecay => _profile != null ? _profile.awarenessDecayPerSecond : .7f;
        private float ChaseMemory => _profile != null ? _profile.chaseMemory : 4f;
        private float SearchDuration => _profile != null ? _profile.searchDuration : 7f;
        private float AttackRange => _profile != null ? _profile.attackRange : 1.45f;
        private float AttackDamage => (_profile != null ? _profile.attackDamage : 45f) * SurvivalDifficultyService.EnemyDamage;
        private float AttackCooldown => (_profile != null ? _profile.attackCooldown : 1.4f) * SurvivalDifficultyService.EnemyAttackCooldown;

        private void Awake()
        {
            if (!GameFeatures.IsEnabled(OptionalGameFeature.EnemyAI)) { gameObject.SetActive(false); return; }
            _agent = GetComponent<NavMeshAgent>();
            _spawnPosition = transform.position;
            _spawnRotation = transform.rotation;
            if (_eye == null) _eye = transform;
        }

        private void OnEnable()
        {
            GameplayNoise.Emitted += HandleNoise;
            if (CheckpointManager.Instance != null) CheckpointManager.Instance.Respawned += ResetEnemy;
            ChaseDirector.Instance?.Register(this);
        }

        private void Start()
        {
            ResolvePlayer();
            ChaseDirector.Instance?.Register(this);
            SetState(_patrolPoints.Length > 0 ? HorrorEnemyState.Patrol : HorrorEnemyState.Idle);
        }

        private void OnDisable()
        {
            CancelEnemyTraversal();
            GameplayNoise.Emitted -= HandleNoise;
            if (CheckpointManager.Instance != null) CheckpointManager.Instance.Respawned -= ResetEnemy;
            ChaseDirector.Instance?.Unregister(this);
        }

        private void Update()
        {
            if (IsTraversing) return;
            if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh) return;
            if (_player == null) ResolvePlayer();
            if (Time.time >= _nextPerception)
            {
                _nextPerception = Time.time + PerceptionInterval;
                if (Time.time >= _detectionSuppressedUntil && CanSeePlayer(out float visibility))
                {
                    _lastKnownPosition = _player.position;
                    _lastSeenTime = Time.time;
                    float distance = Vector3.Distance(_eye.position, _player.position);
                    float instantRange = _profile != null ? _profile.instantDetectionRange : 2.2f;
                    SetAwareness(distance <= instantRange
                        ? 1f
                        : _awareness + PerceptionInterval / Mathf.Max(.01f, DetectionSeconds) * visibility);
                    if (_awareness >= 1f) SetState(HorrorEnemyState.Chase);
                    else if (_state is HorrorEnemyState.Idle or HorrorEnemyState.Patrol or HorrorEnemyState.Return)
                    {
                        SetState(HorrorEnemyState.Suspicious);
                        SetDestination(_lastKnownPosition, InvestigateSpeed);
                    }
                }
                else if (_state != HorrorEnemyState.Chase)
                    SetAwareness(_awareness - AwarenessDecay * PerceptionInterval);
            }

            if (_state is HorrorEnemyState.Patrol or HorrorEnemyState.Suspicious or HorrorEnemyState.Investigate
                or HorrorEnemyState.Chase or HorrorEnemyState.Return)
            {
                if (TryOperateTraversal()) return;
            }

            if (_state is HorrorEnemyState.Chase or HorrorEnemyState.Investigate)
                TryOperateDoor();

            switch (_state)
            {
                case HorrorEnemyState.Patrol: UpdatePatrol(); break;
                case HorrorEnemyState.Suspicious:
                case HorrorEnemyState.Investigate: UpdateInvestigation(); break;
                case HorrorEnemyState.Search: UpdateSearch(); break;
                case HorrorEnemyState.Chase: UpdateChase(); break;
                case HorrorEnemyState.Return: UpdateReturn(); break;
            }
        }

        private void ResolvePlayer()
        {
            _vitals = PlayerVitals.Instance != null ? PlayerVitals.Instance : FindAnyObjectByType<PlayerVitals>();
            _player = _vitals != null ? _vitals.transform : null;
            _visibility = _player != null ? _player.GetComponent<PlayerVisibility>() : null;
        }

        private bool CanSeePlayer(out float visibility)
        {
            visibility = _visibility != null && (_profile == null || _profile.useVisibilityModifiers)
                ? _visibility.CurrentMultiplier : 1f;
            if (_player == null || (_vitals != null && _vitals.IsHidden)) return false;
            Vector3 origin = _eye.position;
            Vector3 target = _player.position + Vector3.up * 1.2f;
            Vector3 toTarget = target - origin;
            float effectiveRange = SightRange * Mathf.Clamp(visibility, .25f, 1.5f);
            if (toTarget.sqrMagnitude > effectiveRange * effectiveRange) return false;
            float minimumDot = Mathf.Cos(SightAngle * .5f * Mathf.Deg2Rad);
            if (Vector3.Dot(_eye.forward, toTarget.normalized) < minimumDot) return false;
            if (!Physics.Raycast(origin, toTarget.normalized, out RaycastHit hit, toTarget.magnitude, _visionBlockingMask, QueryTriggerInteraction.Ignore)) return true;
            return hit.transform == _player || hit.transform.IsChildOf(_player);
        }

        private void HandleNoise(GameplayNoiseStimulus noise)
        {
            if (!isActiveAndEnabled || _state == HorrorEnemyState.Chase) return;
            if ((noise.Position - transform.position).sqrMagnitude > Mathf.Pow(noise.Radius * Hearing, 2f)) return;
            _lastKnownPosition = noise.Position;
            float minimumAwareness = noise.Type switch
            {
                GameplayNoiseType.DoorCareful => .16f,
                GameplayNoiseType.DoorSlam => .65f,
                GameplayNoiseType.Impact => .55f,
                _ => .3f
            };
            SetAwareness(Mathf.Max(_awareness, minimumAwareness));
            SetState(HorrorEnemyState.Suspicious);
            SetDestination(_lastKnownPosition, InvestigateSpeed);
        }

        private void UpdatePatrol()
        {
            if (_patrolPoints.Length == 0) { SetState(HorrorEnemyState.Idle); return; }
            if (!_agent.hasPath) SetDestination(_patrolPoints[_patrolIndex].position, PatrolSpeed);
            if (_agent.pathPending || _agent.remainingDistance > _waypointTolerance) return;
            _patrolIndex = (_patrolIndex + 1) % _patrolPoints.Length;
            SetDestination(_patrolPoints[_patrolIndex].position, PatrolSpeed);
        }

        private void UpdateInvestigation()
        {
            if (!_agent.hasPath) SetDestination(_lastKnownPosition, InvestigateSpeed);
            if (_agent.pathPending || _agent.remainingDistance > _waypointTolerance) return;
            if (_suspectedHidingSpot != null && _suspectedHidingSpot.IsOccupied)
            {
                float distance = Vector3.Distance(transform.position, _suspectedHidingSpot.InspectionPosition);
                if (distance > _waypointTolerance + .5f)
                {
                    SetDestination(_suspectedHidingSpot.InspectionPosition, InvestigateSpeed);
                    return;
                }
                if (Time.time < _inspectHidingAt) return;
                if (_profile == null || _profile.inspectHidingSpots)
                {
                    _suspectedHidingSpot.ForceExpose();
                    _suspectedHidingSpot = null;
                    SetAwareness(1f);
                    SetState(HorrorEnemyState.Chase);
                    return;
                }
            }
            _searchUntil = Time.time + SearchDuration;
            SetState(HorrorEnemyState.Search);
        }

        private void UpdateSearch()
        {
            if (Time.time < _searchUntil) return;
            SetState(_patrolPoints.Length > 0 ? HorrorEnemyState.Return : HorrorEnemyState.Idle);
            _agent.ResetPath();
        }

        private void UpdateReturn()
        {
            if (_patrolPoints.Length == 0) { SetState(HorrorEnemyState.Idle); return; }
            if (!_agent.hasPath) SetDestination(_patrolPoints[_patrolIndex].position, PatrolSpeed);
            if (_agent.pathPending || _agent.remainingDistance > _waypointTolerance) return;
            SetState(HorrorEnemyState.Patrol);
        }

        private void UpdateChase()
        {
            if (_player == null) return;
            if (_vitals != null && _vitals.IsHidden)
            {
                _suspectedHidingSpot = HidingSpot.ActiveForPlayer;
                _inspectHidingAt = Time.time + (_profile != null ? _profile.hidingInspectionDelay : 1.1f)
                    * SurvivalDifficultyService.HidingInspectionDelay;
                SetState(HorrorEnemyState.Investigate);
                SetDestination(_suspectedHidingSpot != null ? _suspectedHidingSpot.InspectionPosition : _lastKnownPosition, InvestigateSpeed);
                return;
            }

            if (Time.time - _lastSeenTime > ChaseMemory)
            {
                SetState(HorrorEnemyState.Investigate);
                SetDestination(_lastKnownPosition, InvestigateSpeed);
                return;
            }

            _lastKnownPosition = _player.position;
            SetDestination(_lastKnownPosition, ChaseSpeed);
            if (Vector3.Distance(transform.position, _player.position) <= AttackRange && Time.time >= _nextAttack)
            {
                _nextAttack = Time.time + AttackCooldown;
                _vitals?.ApplyDamage(new DamageInfo(AttackDamage, DamageType.Enemy, gameObject, transform.position));
            }
        }

        private void SetDestination(Vector3 destination, float speed)
        {
            if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh) return;
            _agent.speed = speed;
            _agent.SetDestination(destination);
        }

        private bool TryOperateTraversal()
        {
            if (Time.time < _nextTraversalAttempt || _agent == null) return false;
            Vector3 direction = _agent.desiredVelocity;
            direction.y = 0f;
            if (direction.sqrMagnitude < .01f) direction = transform.forward;
            direction.Normalize();
            Vector3 origin = transform.position + Vector3.up * Mathf.Max(.5f, _agent.height * .4f)
                + direction * (_agent.radius + .05f);
            if (!Physics.SphereCast(origin, _traversalDetectionRadius, direction, out RaycastHit hit,
                    _traversalDetectionDistance, ~0, QueryTriggerInteraction.Ignore)) return false;
            TraversalObstacle obstacle = hit.transform.GetComponentInParent<TraversalObstacle>();
            if (obstacle == null || !obstacle.AllowsEnemyTraversal) return false;
            _nextTraversalAttempt = Time.time + _traversalRetryDelay;
            return TryBeginEnemyTraversal(obstacle);
        }

        /// <summary>Starts a visible traversal instead of teleporting the enemy across the obstacle.</summary>
        public bool TryBeginEnemyTraversal(TraversalObstacle obstacle)
        {
            if (obstacle == null || !obstacle.AllowsEnemyTraversal || IsTraversing
                || _agent == null || !_agent.enabled || !_agent.isOnNavMesh) return false;

            _activeTraversal = obstacle;
            _traversalSafePosition = transform.position;
            _traversalSafeRotation = transform.rotation;
            obstacle.ResolvePath(transform.position, out Vector3 entryPosition, out Quaternion entryRotation,
                out Vector3 exitPosition, out Quaternion exitRotation);
            _agent.isStopped = true;
            _agent.ResetPath();
            _agent.enabled = false;
            obstacle.NotifyEnemyStarted();
            _traversalRoutine = StartCoroutine(TraverseObstacle(obstacle, entryPosition, entryRotation,
                exitPosition, exitRotation));
            return true;
        }

        public void CancelEnemyTraversal()
        {
            if (!IsTraversing) return;
            TraversalObstacle cancelled = _activeTraversal;
            StopCoroutine(_traversalRoutine);
            _traversalRoutine = null;
            RestoreAgent(_traversalSafePosition, _traversalSafeRotation);
            _activeTraversal = null;
            cancelled?.NotifyEnemyCancelled();
        }

        private IEnumerator TraverseObstacle(TraversalObstacle obstacle, Vector3 entryPosition,
            Quaternion entryRotation, Vector3 exitPosition, Quaternion exitRotation)
        {
            Vector3 startPosition = transform.position;
            Quaternion startRotation = transform.rotation;
            float duration = Mathf.Max(.05f, obstacle.Duration);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (obstacle == null)
                {
                    _traversalRoutine = null;
                    RestoreAgent(_traversalSafePosition, _traversalSafeRotation);
                    _activeTraversal = null;
                    yield break;
                }
                elapsed += Time.deltaTime;
                float normalized = Mathf.Clamp01(elapsed / duration);
                transform.SetPositionAndRotation(
                    obstacle.EvaluatePosition(startPosition, entryPosition, exitPosition, normalized),
                    obstacle.EvaluateRotation(startRotation, entryRotation, exitRotation, normalized));
                yield return null;
            }

            _traversalRoutine = null;
            RestoreAgent(exitPosition, exitRotation);
            _activeTraversal = null;
            obstacle.NotifyEnemyCompleted();
        }

        private void RestoreAgent(Vector3 desiredPosition, Quaternion desiredRotation)
        {
            transform.SetPositionAndRotation(desiredPosition, desiredRotation);
            if (_agent == null) return;
            int areaMask = _agent.areaMask;
            if (NavMesh.SamplePosition(desiredPosition, out NavMeshHit hit, 1.5f, areaMask))
                transform.position = hit.position;
            _agent.enabled = true;
            if (_agent.isOnNavMesh)
            {
                _agent.Warp(transform.position);
                _agent.isStopped = false;
            }
        }

        private void TryOperateDoor()
        {
            if ((_profile != null && !_profile.operateDoors) || Time.time < _nextDoorInteraction) return;
            float distance = _profile != null ? _profile.doorInteractionDistance : 1.35f;
            Vector3 origin = transform.position + Vector3.up * .8f;
            if (!Physics.Raycast(origin, transform.forward, out RaycastHit hit, distance, ~0, QueryTriggerInteraction.Ignore)) return;
            Door door = hit.transform.GetComponentInParent<Door>();
            if (door == null) return;
            _nextDoorInteraction = Time.time + (_profile != null ? _profile.doorInteractionCooldown : .8f);
            DoorOperationMode mode = _state == HorrorEnemyState.Chase
                && (_profile == null || _profile.slamDoorsDuringChase)
                ? DoorOperationMode.Slam
                : DoorOperationMode.Normal;
            door.TryOpenForAI(_profile != null && _profile.forceLockedDoors, mode);
        }

        private void SetAwareness(float value)
        {
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(_awareness, value)) return;
            _awareness = value;
            AwarenessChanged?.Invoke(value);
        }

        public void ForceLosePlayer(float suppressionSeconds = 0f)
        {
            _detectionSuppressedUntil = Mathf.Max(_detectionSuppressedUntil, Time.time + Mathf.Max(0f, suppressionSeconds));
            _lastSeenTime = float.NegativeInfinity;
            _suspectedHidingSpot = null;
            SetAwareness(0f);
            SetState(HorrorEnemyState.Search);
            _searchUntil = Time.time + SearchDuration;
            if (_agent != null && _agent.isOnNavMesh) _agent.ResetPath();
        }

        private void SetState(HorrorEnemyState value)
        {
            if (_state == value) return;
            bool wasChasing = IsChasing;
            _state = value;
            StateChanged?.Invoke(value);
            if (wasChasing != IsChasing) ChaseChanged?.Invoke(IsChasing);
        }

        public void ResetEnemy()
        {
            CancelEnemyTraversal();
            if (_agent != null && _agent.enabled && _agent.isOnNavMesh) _agent.Warp(_spawnPosition);
            else transform.position = _spawnPosition;
            transform.rotation = _spawnRotation;
            _lastSeenTime = float.NegativeInfinity;
            _detectionSuppressedUntil = 0f;
            _suspectedHidingSpot = null;
            SetAwareness(0f);
            if (_agent != null && _agent.enabled && _agent.isOnNavMesh) _agent.ResetPath();
            SetState(_patrolPoints.Length > 0 ? HorrorEnemyState.Patrol : HorrorEnemyState.Idle);
        }

        public void Configure(HorrorEnemyProfile profile, Transform eye, Transform[] patrolPoints)
        {
            _profile = profile; _eye = eye; _patrolPoints = patrolPoints ?? Array.Empty<Transform>();
        }

        private void OnDrawGizmosSelected()
        {
            Transform origin = _eye != null ? _eye : transform;
            Gizmos.color = new Color(1f, .65f, .1f, .2f);
            Gizmos.DrawWireSphere(origin.position, SightRange);
            Vector3 left = Quaternion.AngleAxis(-SightAngle * .5f, Vector3.up) * origin.forward;
            Vector3 right = Quaternion.AngleAxis(SightAngle * .5f, Vector3.up) * origin.forward;
            Gizmos.DrawLine(origin.position, origin.position + left * SightRange);
            Gizmos.DrawLine(origin.position, origin.position + right * SightRange);
            if (_lastKnownPosition != Vector3.zero)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(_lastKnownPosition, .35f);
            }
        }
    }
}
