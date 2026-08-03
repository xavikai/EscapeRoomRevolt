using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EscapeRoomRevolt.Core.Settings;

namespace EscapeRoomRevolt.Systems.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Global Volumes")]
        [Tooltip("Used only when no GameSettingsService is present (e.g. a standalone test scene). Otherwise the player's saved settings win.")]
        [Range(0f, 1f)] public float MasterVolume = 1f;
        [Range(0f, 1f)] public float MusicVolume = 0.5f;
        [Range(0f, 1f)] public float SFXVolume = 1f;

        private float EffectiveMasterVolume => GameSettingsService.Instance != null ? GameSettingsService.Instance.Data.masterVolume : MasterVolume;
        private float EffectiveMusicVolume => GameSettingsService.Instance != null ? GameSettingsService.Instance.Data.musicVolume : MusicVolume;
        private float EffectiveSFXVolume => GameSettingsService.Instance != null ? GameSettingsService.Instance.Data.sfxVolume : SFXVolume;

        [Header("Pool Settings")]
        [SerializeField] private int _poolSize = 15;
        
        private Queue<AudioSource> _audioSourcePool;
        private GameObject _poolContainer;

        // BGM & Ambient Sources
        private AudioSource _bgmSourceA;
        private AudioSource _bgmSourceB;
        private AudioSource _ambientSource;
        private bool _isBgmA_Active = true;

        private Coroutine _bgmCrossfadeRoutine;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSystems();
            }
            else
            {
                Destroy(this);
            }
        }

        private void InitializeSystems()
        {
            // Init SFX Pool
            _audioSourcePool = new Queue<AudioSource>();
            _poolContainer = new GameObject("AudioSourcePool");
            _poolContainer.transform.SetParent(transform);

            for (int i = 0; i < _poolSize; i++)
            {
                CreateNewAudioSource();
            }

            // Init BGM Sources (2D)
            _bgmSourceA = gameObject.AddComponent<AudioSource>();
            _bgmSourceA.loop = true;
            _bgmSourceA.spatialBlend = 0f;

            _bgmSourceB = gameObject.AddComponent<AudioSource>();
            _bgmSourceB.loop = true;
            _bgmSourceB.spatialBlend = 0f;

            // Init Ambient Source (2D)
            _ambientSource = gameObject.AddComponent<AudioSource>();
            _ambientSource.loop = true;
            _ambientSource.spatialBlend = 0f;
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

        // ── SFX ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Plays an AudioClip at a specific world position.
        /// </summary>
        public void PlaySoundAt(AudioClip clip, Vector3 position, float volumeMultiplier = 1f, float pitchVariance = 0f)
        {
            if (clip == null) return;

            AudioSource source = GetAvailableSource();
            source.transform.position = position;
            source.clip = clip;
            source.volume = volumeMultiplier * EffectiveSFXVolume * EffectiveMasterVolume;

            source.spatialBlend = 1f; // Ensure it's 3D
            source.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
            
            source.gameObject.SetActive(true);
            source.Play();

            StartCoroutine(ReturnToPoolAfterDelay(source, clip.length));
        }

        /// <summary>
        /// Plays a 2D voice clip (like internal thoughts or radio).
        /// </summary>
        public void PlayVoice(AudioClip clip, float volumeMultiplier = 1f)
        {
            if (clip == null) return;

            AudioSource source = GetAvailableSource();
            source.clip = clip;
            source.volume = volumeMultiplier * EffectiveSFXVolume * EffectiveMasterVolume;

            source.spatialBlend = 0f; // 2D Sound, no panning/distance attenuation
            source.pitch = 1f;
            
            source.gameObject.SetActive(true);
            source.Play();

            StartCoroutine(ReturnToPoolAfterDelay(source, clip.length));
        }

        private AudioSource GetAvailableSource()
        {
            if (_audioSourcePool.Count > 0)
                return _audioSourcePool.Dequeue();
            
            return CreateNewAudioSource();
        }

        private IEnumerator ReturnToPoolAfterDelay(AudioSource source, float delay)
        {
            yield return new WaitForSeconds(delay);
            source.gameObject.SetActive(false);
            source.clip = null;
            _audioSourcePool.Enqueue(source);
        }

        // ── BGM & AMBIENT ────────────────────────────────────────────────────

        /// <summary>
        /// Crossfades to a new Background Music track with default duration (1.5s). Required for UnityEvents.
        /// </summary>
        public void PlayBGM(AudioClip clip)
        {
            PlayBGM(clip, 1.5f);
        }

        /// <summary>
        /// Crossfades to a new Background Music track.
        /// </summary>
        public void PlayBGM(AudioClip clip, float crossfadeDuration)
        {
            if (clip == null) return;

            AudioSource activeSource = _isBgmA_Active ? _bgmSourceA : _bgmSourceB;
            if (activeSource.clip == clip) return; // Already playing this track

            if (_bgmCrossfadeRoutine != null) StopCoroutine(_bgmCrossfadeRoutine);
            _bgmCrossfadeRoutine = StartCoroutine(CrossfadeRoutine(clip, crossfadeDuration));
        }

        /// <summary>
        /// Plays an ambient loop (like room tone, wind, or breathing).
        /// </summary>
        public void PlayAmbient(AudioClip clip, float volumeMultiplier = 1f)
        {
            if (_ambientSource.clip == clip) return;
            _ambientSource.clip = clip;
            _ambientSource.volume = volumeMultiplier * EffectiveSFXVolume * EffectiveMasterVolume;
            _ambientSource.Play();
        }

        private IEnumerator CrossfadeRoutine(AudioClip newClip, float duration)
        {
            AudioSource fadeOutSource = _isBgmA_Active ? _bgmSourceA : _bgmSourceB;
            AudioSource fadeInSource  = _isBgmA_Active ? _bgmSourceB : _bgmSourceA;

            _isBgmA_Active = !_isBgmA_Active;

            fadeInSource.clip = newClip;
            fadeInSource.volume = 0f;
            fadeInSource.Play();

            float elapsed = 0f;
            float startVolumeOut = fadeOutSource.volume;
            float targetVolumeIn = EffectiveMusicVolume * EffectiveMasterVolume;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                fadeOutSource.volume = Mathf.Lerp(startVolumeOut, 0f, t);
                fadeInSource.volume  = Mathf.Lerp(0f, targetVolumeIn, t);

                yield return null;
            }

            fadeOutSource.Stop();
            fadeOutSource.clip = null;
        }

        private void Update()
        {
            // Real-time volume adjustments (in case they are changed via a Settings Menu)
            if (_isBgmA_Active && _bgmSourceA.isPlaying) _bgmSourceA.volume = EffectiveMusicVolume * EffectiveMasterVolume;
            if (!_isBgmA_Active && _bgmSourceB.isPlaying) _bgmSourceB.volume = EffectiveMusicVolume * EffectiveMasterVolume;
            if (_ambientSource.isPlaying) _ambientSource.volume = EffectiveSFXVolume * EffectiveMasterVolume;
        }
    }
}
