using System;
using System.Collections;
using EscapeRoomRevolt.Core.Save;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EscapeRoomRevolt.Core.Flow
{
    public enum GameFlowState { Boot, MainMenu, Loading, Playing, Paused, Completed, Failed }
    public enum GameOutcome { Victory, Defeat }

    public readonly struct GameResult
    {
        public readonly GameOutcome Outcome;
        public readonly string EndingId;
        public readonly string Title;
        public readonly string Message;

        public GameResult(GameOutcome outcome, string endingId, string title, string message)
        {
            Outcome = outcome;
            EndingId = endingId;
            Title = title;
            Message = message;
        }
    }

    /// <summary>Persistent authority for scene routing, pause state and endings. It owns no presentation.</summary>
    [DefaultExecutionOrder(-90)]
    public sealed class GameFlowManager : MonoBehaviour
    {
        private const string SettingsResourcePath = "GameFlowSettings";

        public static GameFlowManager Instance { get; private set; }
        public static GameFlowState State => Instance != null ? Instance._state : GameFlowState.Boot;
        public static GameResult? LastResult => Instance != null ? Instance._lastResult : null;

        [SerializeField] private GameFlowSettings _settings;
        private GameFlowState _state = GameFlowState.Boot;
        private GameResult? _lastResult;
        private bool _transitionInProgress;

        public event Action<GameFlowState> StateChanged;
        public event Action<GameResult> GameEnded;

        public bool IsMainMenuScene => MatchesScene(SceneManager.GetActiveScene(), MainMenuScene);
        public string MainMenuScene => _settings != null ? _settings.MainMenuScene : "MainMenu";
        public string FirstGameplayScene => _settings != null ? _settings.FirstGameplayScene : "ShowcaseMuseum";

        public static GameFlowManager EnsureInstance()
        {
            if (Instance != null) return Instance;
            GameFlowManager existing = FindAnyObjectByType<GameFlowManager>();
            if (existing != null) return existing;
            return new GameObject("GameFlowManager").AddComponent<GameFlowManager>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (_settings == null) _settings = Resources.Load<GameFlowSettings>(SettingsResourcePath);
            SceneManager.sceneLoaded += HandleSceneLoaded;
            SetState(IsMainMenuScene ? GameFlowState.MainMenu : GameFlowState.Playing);
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            if (Instance == this) Instance = null;
        }

        public void StartNewGame()
        {
            if (_transitionInProgress) return;
            _lastResult = null;
            SaveManager.Instance?.ResetSession();
            GameContext.ResetForNewSession();
            LoadScene(FirstGameplayScene);
        }

        public bool CanContinue()
        {
            SaveManager save = SaveManager.Instance;
            return save != null && !string.IsNullOrEmpty(ResolveContinueSlot(save));
        }

        public void ContinueGame()
        {
            if (_transitionInProgress) return;
            SaveManager save = SaveManager.Instance;
            string slot = save != null ? ResolveContinueSlot(save) : null;
            if (save == null || string.IsNullOrEmpty(slot)) return;
            _lastResult = null;
            SetState(GameFlowState.Loading);
            save.LoadGame(slot);
        }

        public void LoadSlot(string slotId)
        {
            if (_transitionInProgress || SaveManager.Instance == null) return;
            _lastResult = null;
            SetState(GameFlowState.Loading);
            SaveManager.Instance.LoadGame(slotId);
        }

        public void ReturnToMainMenu()
        {
            if (_transitionInProgress) return;
            Time.timeScale = 1f;
            LoadScene(MainMenuScene);
        }

        public void RestartCurrentScene()
        {
            if (_transitionInProgress) return;
            Time.timeScale = 1f;
            LoadScene(SceneManager.GetActiveScene().path);
        }

        public void SetPaused(bool paused)
        {
            if (_state == GameFlowState.Completed || _state == GameFlowState.Failed || _state == GameFlowState.Loading) return;
            Time.timeScale = paused ? 0f : 1f;
            SetState(paused ? GameFlowState.Paused : GameFlowState.Playing);
        }

        public void CompleteGame(EndingDefinition ending = null)
        {
            Finish(new GameResult(GameOutcome.Victory,
                ending != null ? ending.EndingId : "escaped",
                ending != null ? ending.Title : "Has escapado",
                ending != null ? ending.Message : "La investigación ha terminado."));
        }

        public void FailGame(EndingDefinition ending = null)
        {
            Finish(new GameResult(GameOutcome.Defeat,
                ending != null ? ending.EndingId : "failed",
                ending != null ? ending.Title : "Fin de la investigación",
                ending != null ? ending.Message : "La investigación no pudo continuar."));
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void Finish(GameResult result)
        {
            if (_state == GameFlowState.Completed || _state == GameFlowState.Failed) return;
            _lastResult = result;
            Time.timeScale = 0f;
            SetState(result.Outcome == GameOutcome.Victory ? GameFlowState.Completed : GameFlowState.Failed);
            GameEnded?.Invoke(result);
            EventBus.Publish(new OnGameEnded
            {
                outcome = result.Outcome,
                endingId = result.EndingId,
                title = result.Title,
                message = result.Message
            });
        }

        private void LoadScene(string scene)
        {
            if (string.IsNullOrWhiteSpace(scene))
            {
                Debug.LogError("[GameFlow] No scene is configured for this transition.");
                return;
            }
            StartCoroutine(LoadSceneRoutine(scene));
        }

        private IEnumerator LoadSceneRoutine(string scene)
        {
            _transitionInProgress = true;
            SetState(GameFlowState.Loading);
            AsyncOperation operation = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
            if (operation == null)
            {
                Debug.LogError($"[GameFlow] Unity could not load scene '{scene}'. Add it to Build Settings.");
                _transitionInProgress = false;
                yield break;
            }
            while (!operation.isDone) yield return null;
            _transitionInProgress = false;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _transitionInProgress = false;
            Time.timeScale = 1f;
            SetState(MatchesScene(scene, MainMenuScene) ? GameFlowState.MainMenu : GameFlowState.Playing);
        }

        private void SetState(GameFlowState state)
        {
            if (_state == state) return;
            _state = state;
            StateChanged?.Invoke(state);
            EventBus.Publish(new OnGameFlowStateChanged { state = state });
        }

        private string ResolveContinueSlot(SaveManager save)
        {
            string preferred = _settings != null ? _settings.ContinueSlot : string.Empty;
            if (!string.IsNullOrWhiteSpace(preferred) && save.HasSave(preferred)) return preferred;
            var slots = save.GetSlots();
            return slots.Count > 0 ? slots[0].slotId : null;
        }

        private static bool MatchesScene(Scene scene, string configuredScene)
        {
            if (string.IsNullOrWhiteSpace(configuredScene)) return false;
            return string.Equals(scene.name, configuredScene, StringComparison.OrdinalIgnoreCase)
                || string.Equals(scene.path, configuredScene, StringComparison.OrdinalIgnoreCase);
        }
    }
}
