using UnityEngine;
using System.Collections.Generic;
using EscapeRoomRevolt.Systems.Interaction;

namespace EscapeRoomRevolt.Systems.Hint
{
    /// <summary>
    /// A trigger that tells the HintManager which puzzle context the player is currently in.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class HintZoneTrigger : MonoBehaviour
    {
        [Tooltip("The hint data for this specific area or puzzle.")]
        [SerializeField] private HintData _puzzleHintData;

        [Tooltip("If true, the hint context will be cleared when the player leaves the trigger.")]
        [SerializeField] private bool _clearOnExit = false;

        private readonly HashSet<Collider> _occupants = new HashSet<Collider>();

        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
            var body = GetComponent<Rigidbody>();
            if (body == null) body = gameObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (PlayerTriggerUtility.IsPlayer(other) && _occupants.Add(other) && _occupants.Count == 1)
            {
                if (_puzzleHintData != null)
                {
                    HintManager.Instance?.SetActivePuzzle(_puzzleHintData);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (_occupants.Remove(other) && _occupants.Count == 0 && _clearOnExit)
            {
                HintManager.Instance?.ClearActivePuzzle(_puzzleHintData);
            }
        }

        private void OnDisable()
        {
            if (_occupants.Count > 0 && _clearOnExit)
                HintManager.Instance?.ClearActivePuzzle(_puzzleHintData);
            _occupants.Clear();
        }
    }
}
