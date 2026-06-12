using UnityEngine;
using System.Collections.Generic;

namespace EscapeRoomRevolt.Systems.Audio
{
    [System.Serializable]
    public class SurfaceAudioMapping
    {
        public string SurfaceTag; // e.g., "Wood", "Stone", "Metal", "Carpet"
        public AudioClip[] FootstepClips;
    }

    [CreateAssetMenu(fileName = "NewSurfaceAudioData", menuName = "EscapeRoom/Audio/Surface Data", order = 1)]
    public class SurfaceAudioData : ScriptableObject
    {
        [Header("Default Sound (if no tag matches)")]
        public AudioClip[] DefaultFootsteps;

        [Header("Surface Mappings")]
        public List<SurfaceAudioMapping> SurfaceMappings = new List<SurfaceAudioMapping>();

        public AudioClip GetRandomClip(string tag)
        {
            foreach (var mapping in SurfaceMappings)
            {
                if (mapping.SurfaceTag == tag && mapping.FootstepClips != null && mapping.FootstepClips.Length > 0)
                {
                    return mapping.FootstepClips[Random.Range(0, mapping.FootstepClips.Length)];
                }
            }
            
            // Fallback
            if (DefaultFootsteps != null && DefaultFootsteps.Length > 0)
            {
                return DefaultFootsteps[Random.Range(0, DefaultFootsteps.Length)];
            }

            return null;
        }
    }
}
