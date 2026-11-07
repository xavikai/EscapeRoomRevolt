using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.Video;

namespace EscapeRoomRevolt.Core.Flow
{
    public enum CutsceneStepKind
    {
        /// <summary>A still frame: company logo, title card, hand-drawn panel.</summary>
        Image,
        /// <summary>A pre-rendered video file played full screen.</summary>
        Video,
        /// <summary>An in-engine shot: switches to a camera already placed in the scene.</summary>
        SceneCamera
    }

    [Serializable]
    public sealed class CutsceneStep
    {
        public CutsceneStepKind kind = CutsceneStepKind.Image;

        [Tooltip("Shown full screen, letterboxed to preserve its aspect ratio. Used when Kind is Image.")]
        public Sprite image;
        [Tooltip("Used when Kind is Video. Its own length wins unless Duration is greater than zero.")]
        public VideoClip video;
        [Tooltip("Used when Kind is Scene Camera. Enabled for Duration seconds, then switched back off.")]
        public Camera sceneCamera;

        [Tooltip("Seconds this step stays on screen. For video, leave at 0 to use the clip's own length.")]
        [Min(0f)] public float duration = 3f;
        [Tooltip("Seconds to fade in and out of black around this step.")]
        [Min(0f)] public float fadeDuration = .5f;
        [Tooltip("Optional audio for this step, e.g. a sting under a logo.")]
        public AudioClip audioClip;
    }

    /// <summary>
    /// Plays an ordered list of cutscene steps — stills, video files and in-engine camera shots —
    /// over a full-screen overlay, then reports it is done. Used for the intro before the main menu
    /// and for an ending cinematic before the results screen, but it has no knowledge of either:
    /// it just plays and raises OnFinished, so it can sit anywhere.
    ///
    /// Runs on unscaled time, because an ending cutscene plays while the game is already frozen.
    /// </summary>
    public sealed class CutsceneSequence : MonoBehaviour
    {
        [SerializeField] private List<CutsceneStep> _steps = new List<CutsceneStep>();
        [SerializeField] private bool _playOnStart;
        [Tooltip("Any key or button skips to the end. Leave on for anything a player may watch twice.")]
        [SerializeField] private bool _skippable = true;
        [Tooltip("Panel settings used for the overlay. Falls back to the one shipped with the framework.")]
        [SerializeField] private PanelSettings _panelSettings;

        [Header("Events")]
        public UnityEvent OnStarted;
        public UnityEvent OnFinished;

        private UIDocument _document;
        private VisualElement _overlay;
        private VisualElement _picture;
        private VideoPlayer _videoPlayer;
        private AudioSource _audioSource;
        private Coroutine _routine;

        public bool IsPlaying { get; private set; }
        public int StepCount => _steps.Count;

        private void Start()
        {
            if (_playOnStart) Play();
        }

        /// <summary>Starts the sequence. Safe to call from a UnityEvent. Does nothing while already playing.</summary>
        public void Play()
        {
            if (IsPlaying) return;
            if (_steps.Count == 0) { OnFinished?.Invoke(); return; }
            _routine = StartCoroutine(PlayRoutine());
        }

        /// <summary>Cuts to the end immediately, as if the player had skipped it.</summary>
        public void Skip()
        {
            if (!IsPlaying) return;
            if (_routine != null) StopCoroutine(_routine);
            Cleanup();
            IsPlaying = false;
            OnFinished?.Invoke();
        }

        private IEnumerator PlayRoutine()
        {
            IsPlaying = true;
            BuildOverlay();
            OnStarted?.Invoke();

            foreach (CutsceneStep step in _steps)
            {
                if (WasSkipped()) break;
                yield return PlayStep(step);
            }

            Cleanup();
            IsPlaying = false;
            OnFinished?.Invoke();
        }

        private IEnumerator PlayStep(CutsceneStep step)
        {
            if (step.audioClip != null && _audioSource != null) _audioSource.PlayOneShot(step.audioClip);

            switch (step.kind)
            {
                case CutsceneStepKind.SceneCamera:
                    // The overlay would cover the shot, so it stays black only for the fades.
                    if (step.sceneCamera != null) step.sceneCamera.gameObject.SetActive(true);
                    _picture.style.backgroundImage = null;
                    yield return Fade(1f, 0f, step.fadeDuration);
                    yield return Hold(step.duration);
                    yield return Fade(0f, 1f, step.fadeDuration);
                    if (step.sceneCamera != null) step.sceneCamera.gameObject.SetActive(false);
                    break;

                case CutsceneStepKind.Video:
                    if (step.video == null) yield break;
                    EnsureVideoPlayer();
                    _videoPlayer.clip = step.video;
                    _videoPlayer.Play();
                    _picture.style.backgroundImage = null;
                    yield return Fade(1f, 0f, step.fadeDuration);
                    yield return Hold(step.duration > 0f ? step.duration : (float)step.video.length);
                    yield return Fade(0f, 1f, step.fadeDuration);
                    _videoPlayer.Stop();
                    break;

                default:
                    _picture.style.backgroundImage = step.image != null
                        ? new StyleBackground(step.image)
                        : StyleKeyword.None;
                    _overlay.style.opacity = 1f; // the still lives on the overlay, so keep it opaque
                    yield return FadePicture(0f, 1f, step.fadeDuration);
                    yield return Hold(step.duration);
                    yield return FadePicture(1f, 0f, step.fadeDuration);
                    break;
            }
        }

        private IEnumerator Hold(float seconds)
        {
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                if (WasSkipped()) yield break;
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        /// <summary>Fades the black overlay itself, used to reveal the world behind it.</summary>
        private IEnumerator Fade(float from, float to, float seconds) => Lerp(_overlay, from, to, seconds);

        /// <summary>Fades the still image, leaving the black backdrop in place.</summary>
        private IEnumerator FadePicture(float from, float to, float seconds) => Lerp(_picture, from, to, seconds);

        private IEnumerator Lerp(VisualElement element, float from, float to, float seconds)
        {
            if (seconds <= 0f) { element.style.opacity = to; yield break; }
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                if (WasSkipped()) { element.style.opacity = to; yield break; }
                elapsed += Time.unscaledDeltaTime;
                element.style.opacity = Mathf.Lerp(from, to, elapsed / seconds);
                yield return null;
            }
            element.style.opacity = to;
        }

        /// <summary>
        /// Reads the devices directly rather than going through InputRouter: the intro scene runs
        /// before any gameplay services exist, so there is nothing to route through yet.
        /// </summary>
        private bool WasSkipped()
        {
            if (!_skippable) return false;
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) return true;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) return true;
            if (Gamepad.current != null &&
                (Gamepad.current.buttonSouth.wasPressedThisFrame || Gamepad.current.startButton.wasPressedThisFrame)) return true;
            return false;
        }

        private void BuildOverlay()
        {
            _document = gameObject.GetComponent<UIDocument>();
            if (_document == null) _document = gameObject.AddComponent<UIDocument>();
            if (_panelSettings != null) _document.panelSettings = _panelSettings;
            _document.sortingOrder = 1000f; // above the HUD and menus

            _overlay = new VisualElement();
            _overlay.style.position = Position.Absolute;
            _overlay.style.left = 0; _overlay.style.right = 0;
            _overlay.style.top = 0; _overlay.style.bottom = 0;
            _overlay.style.backgroundColor = Color.black;

            _picture = new VisualElement();
            _picture.style.position = Position.Absolute;
            _picture.style.left = 0; _picture.style.right = 0;
            _picture.style.top = 0; _picture.style.bottom = 0;
            _picture.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
            _picture.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);
            _picture.style.backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat);
            _picture.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
            _picture.style.opacity = 0f;
            _overlay.Add(_picture);

            _document.rootVisualElement.Add(_overlay);

            if (_audioSource == null)
            {
                _audioSource = gameObject.GetComponent<AudioSource>();
                if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
                _audioSource.ignoreListenerPause = true; // the ending plays while the game is frozen
            }
        }

        private void EnsureVideoPlayer()
        {
            if (_videoPlayer != null) return;
            _videoPlayer = gameObject.AddComponent<VideoPlayer>();
            _videoPlayer.playOnAwake = false;
            _videoPlayer.isLooping = false;
            _videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
            _videoPlayer.targetCamera = Camera.main;
            _videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        }

        private void Cleanup()
        {
            if (_videoPlayer != null) _videoPlayer.Stop();
            foreach (CutsceneStep step in _steps)
                if (step.kind == CutsceneStepKind.SceneCamera && step.sceneCamera != null)
                    step.sceneCamera.gameObject.SetActive(false);
            if (_overlay != null) _overlay.RemoveFromHierarchy();
        }
    }
}
