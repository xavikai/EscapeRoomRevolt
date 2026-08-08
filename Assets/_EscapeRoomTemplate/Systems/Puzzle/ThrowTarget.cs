using UnityEngine;
using EscapeRoomRevolt.Systems.Interaction;

namespace EscapeRoomRevolt.Systems.Puzzle
{
    /// <summary>
    /// A target for ThrowPuzzle: registers a hit when a thrown PhysicsGrabbable object collides with
    /// it above a minimum speed, so gently bumping into it (e.g. the player walking past) doesn't
    /// count as a throw.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class ThrowTarget : MonoBehaviour
    {
        [SerializeField] private ThrowPuzzle _puzzle;
        [SerializeField] private string _targetId;
        [Tooltip("Minimum impact speed (m/s) for a collision to count as a throw rather than a bump.")]
        [SerializeField] private float _minImpactSpeed = 3f;

        [Header("Visual feedback (optional, uses this object's own Renderer)")]
        [SerializeField] private Color _idleColor = Color.red;
        [SerializeField] private Color _hitColor = Color.green;
        [SerializeField] private Color _solvedColor = Color.cyan;

        private Renderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            if (_puzzle != null) _puzzle.OnSolvedEvent.AddListener(UpdateColor);
            UpdateColor();
        }

        private void OnDestroy()
        {
            if (_puzzle != null) _puzzle.OnSolvedEvent.RemoveListener(UpdateColor);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_puzzle == null || _puzzle.IsSolved || _puzzle.IsTargetHit(_targetId)) return;

            Rigidbody rb = collision.rigidbody;
            if (rb == null || rb.GetComponent<PhysicsGrabbable>() == null) return;
            if (collision.relativeVelocity.magnitude < _minImpactSpeed) return;

            _puzzle.RegisterHit(_targetId);
            UpdateColor();
        }

        private void UpdateColor()
        {
            if (_renderer == null || _puzzle == null) return;
            if (_puzzle.IsSolved) { _renderer.material.color = _solvedColor; return; }
            _renderer.material.color = _puzzle.IsTargetHit(_targetId) ? _hitColor : _idleColor;
        }
    }
}
