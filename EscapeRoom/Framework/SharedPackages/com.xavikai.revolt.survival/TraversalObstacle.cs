using System;
using EscapeRoomRevolt.Core.Settings;
using EscapeRoomRevolt.Systems.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace EscapeRoomRevolt.Systems.Survival
{
    public enum TraversalType { Vault, Climb, Ladder, Squeeze }
    public enum EnemyTraversalPolicy { RouteAround, UseTraversal, Blocked }

    /// <summary>Authorable traversal link. Geometry and animation presentation remain replaceable.</summary>
    [RequireComponent(typeof(Collider))]
    public sealed class TraversalObstacle : InteractableBase
    {
        [SerializeField] private TraversalType _type = TraversalType.Vault;
        [SerializeField] private Transform _entryAnchor;
        [SerializeField] private Transform _exitAnchor;
        [SerializeField, Min(.05f)] private float _duration = .75f;
        [SerializeField, Min(0f)] private float _arcHeight = .75f;
        [SerializeField] private AnimationCurve _motionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private string _prompt = "Travessar";
        [Header("Enemy route")]
        [SerializeField] private EnemyTraversalPolicy _enemyPolicy = EnemyTraversalPolicy.RouteAround;
        [Header("Events")]
        [SerializeField] private UnityEvent _onStarted;
        [SerializeField] private UnityEvent _onCompleted;
        [SerializeField] private UnityEvent _onCancelled;
        [SerializeField] private UnityEvent _onEnemyStarted;
        [SerializeField] private UnityEvent _onEnemyCompleted;
        [SerializeField] private UnityEvent _onEnemyCancelled;

        public TraversalType Type => _type;
        public Vector3 EntryPosition => _entryAnchor != null ? _entryAnchor.position : transform.position;
        public Quaternion EntryRotation => _entryAnchor != null ? _entryAnchor.rotation : transform.rotation;
        public Vector3 ExitPosition => _exitAnchor != null ? _exitAnchor.position : transform.position + transform.forward * 1.5f;
        public Quaternion ExitRotation => _exitAnchor != null ? _exitAnchor.rotation : transform.rotation;
        public float Duration => _duration;
        public float ArcHeight => _type is TraversalType.Squeeze or TraversalType.Ladder ? 0f : _arcHeight;
        public AnimationCurve MotionCurve => _motionCurve;
        public EnemyTraversalPolicy EnemyPolicy => _enemyPolicy;
        public bool AllowsEnemyTraversal => _enemyPolicy == EnemyTraversalPolicy.UseTraversal;
        public override string InteractionPrompt => _prompt;
        public override bool CanInteract => base.CanInteract
            && GameFeatures.IsEnabled(OptionalGameFeature.Traversal)
            && TraversalController.Instance != null
            && !TraversalController.Instance.IsTraversing;

        public event Action<TraversalObstacle> Started;
        public event Action<TraversalObstacle> Completed;
        public event Action<TraversalObstacle> Cancelled;
        public event Action<TraversalObstacle> EnemyStarted;
        public event Action<TraversalObstacle> EnemyCompleted;
        public event Action<TraversalObstacle> EnemyCancelled;

        protected override void Awake()
        {
            base.Awake();
            if (!GameFeatures.IsEnabled(OptionalGameFeature.Traversal)) gameObject.SetActive(false);
            ConfigureNavigationObstacle();
        }

        protected override void OnInteract() => TraversalController.Instance?.TryBegin(this);

        internal void NotifyStarted() { Started?.Invoke(this); _onStarted?.Invoke(); }
        internal void NotifyCompleted() { Completed?.Invoke(this); _onCompleted?.Invoke(); }
        internal void NotifyCancelled() { Cancelled?.Invoke(this); _onCancelled?.Invoke(); }
        internal void NotifyEnemyStarted() { EnemyStarted?.Invoke(this); _onEnemyStarted?.Invoke(); }
        internal void NotifyEnemyCompleted() { EnemyCompleted?.Invoke(this); _onEnemyCompleted?.Invoke(); }
        internal void NotifyEnemyCancelled() { EnemyCancelled?.Invoke(this); _onEnemyCancelled?.Invoke(); }

        /// <summary>Returns the closest anchor as entry, so every obstacle works in both directions.</summary>
        public void ResolvePath(Vector3 origin, out Vector3 entryPosition, out Quaternion entryRotation,
            out Vector3 exitPosition, out Quaternion exitRotation)
        {
            bool reverse = (origin - ExitPosition).sqrMagnitude < (origin - EntryPosition).sqrMagnitude;
            entryPosition = reverse ? ExitPosition : EntryPosition;
            entryRotation = reverse ? Quaternion.LookRotation(-(ExitRotation * Vector3.forward), Vector3.up) : EntryRotation;
            exitPosition = reverse ? EntryPosition : ExitPosition;
            exitRotation = reverse ? Quaternion.LookRotation(-(EntryRotation * Vector3.forward), Vector3.up) : ExitRotation;
        }

        public Vector3 EvaluatePosition(Vector3 startPosition, Vector3 entryPosition, Vector3 exitPosition, float normalizedTime)
        {
            float t = _motionCurve != null ? _motionCurve.Evaluate(Mathf.Clamp01(normalizedTime)) : Mathf.Clamp01(normalizedTime);
            if (t < .25f) return Vector3.Lerp(startPosition, entryPosition, t / .25f);
            float exitT = (t - .25f) / .75f;
            return Vector3.Lerp(entryPosition, exitPosition, exitT)
                + Vector3.up * Mathf.Sin(exitT * Mathf.PI) * ArcHeight;
        }

        public Quaternion EvaluateRotation(Quaternion startRotation, Quaternion entryRotation,
            Quaternion exitRotation, float normalizedTime)
        {
            float t = _motionCurve != null ? _motionCurve.Evaluate(Mathf.Clamp01(normalizedTime)) : Mathf.Clamp01(normalizedTime);
            if (t < .25f) return Quaternion.Slerp(startRotation, entryRotation, t / .25f);
            return Quaternion.Slerp(entryRotation, exitRotation, (t - .25f) / .75f);
        }

        private void ConfigureNavigationObstacle()
        {
            UnityEngine.AI.NavMeshObstacle obstacle = GetComponent<UnityEngine.AI.NavMeshObstacle>();
            if (obstacle != null) obstacle.enabled = _enemyPolicy != EnemyTraversalPolicy.UseTraversal;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 entry = EntryPosition;
            Vector3 exit = ExitPosition;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(entry, .2f);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(exit, .2f);
            Gizmos.color = Color.yellow;
            const int segments = 12;
            Vector3 previous = entry;
            for (int index = 1; index <= segments; index++)
            {
                float t = index / (float)segments;
                Vector3 point = Vector3.Lerp(entry, exit, t) + Vector3.up * Mathf.Sin(t * Mathf.PI) * ArcHeight;
                Gizmos.DrawLine(previous, point);
                previous = point;
            }
        }
    }
}
