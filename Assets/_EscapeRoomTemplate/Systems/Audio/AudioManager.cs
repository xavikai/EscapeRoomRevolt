using UnityEngine;
using System.Collections.Generic;

namespace EscapeRoomRevolt.Systems.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Pool Settings")]
        [SerializeField] private int _poolSize = 15;
        
        private Queue<AudioSource> _audioSourcePool;
        private GameObject _poolContainer;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializePool();
            }
            else
            {
                Destroy(this);
            }
        }

        private void InitializePool()
        {
            _audioSourcePool = new Queue<AudioSource>();
            _poolContainer = new GameObject("AudioSourcePool");
            _poolContainer.transform.SetParent(transform);

            for (int i = 0; i < _poolSize; i++)
            {
                CreateNewAudioSource();
            }
        }

        private AudioSource CreateNewAudioSource()
        {
            GameObject go = new GameObject("PooledAudioSource");
            go.transform.SetParent(_poolContainer.transform);
            AudioSource source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            
            // Default 3D Audio Settings
            source.spatialBlend = 1f; 
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 1f;
            source.maxDistance = 15f;
            
            go.SetActive(false);
            _audioSourcePool.Enqueue(source);
            return source;
        }

        /// <summary>
        /// Plays an AudioClip at a specific world position with optional pitch variation.
        /// </summary>
        /// <param name="clip">The AudioClip to play.</param>
        /// <param name="position">The world position of the sound.</param>
        /// <param name="volume">Volume (0.0 to 1.0).</param>
        /// <param name="pitchVariance">Amount of random pitch variance (e.g. 0.1 means +/- 10% pitch).</param>
        public void PlaySoundAt(AudioClip clip, Vector3 position, float volume = 1f, float pitchVariance = 0f)
        {
            if (clip == null) return;

            AudioSource source = GetAvailableSource();
            source.transform.position = position;
            source.clip = clip;
            source.volume = volume;
            
            // Apply pitch variance (e.g., if variance is 0.1, pitch will be between 0.9 and 1.1)
            source.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
            
            source.gameObject.SetActive(true);
            source.Play();

            // Return to pool after the clip finishes
            StartCoroutine(ReturnToPoolAfterDelay(source, clip.length));
        }

        private AudioSource GetAvailableSource()
        {
            if (_audioSourcePool.Count > 0)
            {
                return _audioSourcePool.Dequeue();
            }
            
            // If pool is empty, expand it
            Debug.LogWarning("[AudioManager] Expanding audio pool!");
            return CreateNewAudioSource();
        }

        private System.Collections.IEnumerator ReturnToPoolAfterDelay(AudioSource source, float delay)
        {
            yield return new WaitForSeconds(delay);
            source.gameObject.SetActive(false);
            source.clip = null;
            _audioSourcePool.Enqueue(source);
        }
    }
}
