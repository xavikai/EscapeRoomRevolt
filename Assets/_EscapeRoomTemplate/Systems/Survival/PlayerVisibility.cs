using System.Collections.Generic;
using EscapeRoomRevolt.Player.PC;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Survival
{
    /// <summary>Platform-neutral visibility signal consumed by enemy perception.</summary>
    public sealed class PlayerVisibility : MonoBehaviour
    {
        [SerializeField, Range(.05f, 2f)] private float _baseVisibility = 1f;
        [SerializeField, Range(.05f, 2f)] private float _crouchedMultiplier = .55f;
        [SerializeField, Range(.05f, 2f)] private float _sprintingMultiplier = 1.25f;
        private readonly List<VisibilityZone> _zones = new List<VisibilityZone>(4);
        private PlayerMovement _movement;
        private PlayerVitals _vitals;

        public float CurrentMultiplier
        {
            get
            {
                if (_vitals != null && _vitals.IsHidden) return .01f;
                float value = _baseVisibility;
                if (_movement != null)
                {
                    if (_movement.IsCrouching) value *= _crouchedMultiplier;
                    else if (_movement.IsSprinting) value *= _sprintingMultiplier;
                }
                for (int index = 0; index < _zones.Count; index++)
                    if (_zones[index] != null) value *= _zones[index].VisibilityMultiplier;
                return Mathf.Clamp(value, .01f, 2f);
            }
        }

        private void Awake()
        {
            _movement = GetComponent<PlayerMovement>();
            _vitals = GetComponent<PlayerVitals>();
        }

        internal void Enter(VisibilityZone zone)
        {
            if (zone != null && !_zones.Contains(zone)) _zones.Add(zone);
        }

        internal void Exit(VisibilityZone zone) => _zones.Remove(zone);
    }

}
