using System;
using System.Collections;
using System.Collections.Generic;
using EscapeRoomRevolt.Core.Save;
using EscapeRoomRevolt.Core.Settings;
using EscapeRoomRevolt.Player;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Survival
{
    [Serializable]
    public sealed class CheckpointSaveState { public string checkpointId; }

    /// <summary>Owns the lightweight death-respawn point; manual save slots remain independent.</summary>
    [DefaultExecutionOrder(-40)]
    public sealed class CheckpointManager : MonoBehaviour, ISaveable
    {
        public static CheckpointManager Instance { get; private set; }
        private SurvivalCheckpoint _current;
        private string _pendingCheckpointId;
        private readonly Dictionary<string, string> _snapshot = new Dictionary<string, string>();
        private readonly Dictionary<string, CheckpointEntityState> _entitySnapshot = new Dictionary<string, CheckpointEntityState>();
        private HashSet<string> _destroyedEntitySnapshot = new HashSet<string>();
        private bool _captureScheduled;

        public event Action<SurvivalCheckpoint> CheckpointReached;
        public event Action Respawned;
        public event Action SnapshotCaptured;
        public SurvivalCheckpoint Current => _current;

        private void Awake()
        {
            if (!GameFeatures.IsEnabled(OptionalGameFeature.Checkpoints)) { enabled = false; return; }
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SaveManager.Instance?.Register(this);
        }

        private void OnDestroy()
        {
            SaveManager.Instance?.Unregister(this);
            if (Instance == this) Instance = null;
        }

        public void Register(SurvivalCheckpoint checkpoint)
        {
            if (checkpoint == null || !SurvivalDifficultyService.AllowsCheckpoints) return;
            if (!string.IsNullOrWhiteSpace(_pendingCheckpointId) && checkpoint.CheckpointId == _pendingCheckpointId)
            {
                _current = checkpoint;
                _pendingCheckpointId = null;
                ScheduleSnapshotCapture();
            }
            else if (_current == null && checkpoint.IsInitial)
            {
                _current = checkpoint;
                ScheduleSnapshotCapture();
            }
        }

        public void Reach(SurvivalCheckpoint checkpoint)
        {
            if (checkpoint == null || _current == checkpoint || !SurvivalDifficultyService.AllowsCheckpoints) return;
            _current = checkpoint;
            CaptureSnapshot();
            CheckpointReached?.Invoke(checkpoint);
        }

        public bool TryRespawn(PlayerVitals vitals)
        {
            if (_current == null || vitals == null || !SurvivalDifficultyService.AllowsCheckpoints) return false;
            HidingSpot.ActiveForPlayer?.ExitImmediately();
            RestoreSnapshot();
            PlayerPlatformRegistry.Current?.TeleportTo(_current.SpawnPosition, _current.SpawnRotation);
            PlayerPlatformRegistry.Current?.SetMovementBlocked(false);
            PlayerPlatformRegistry.Current?.SetLookBlocked(false);
            vitals.RestoreForCheckpoint();
            Respawned?.Invoke();
            return true;
        }

        private void ScheduleSnapshotCapture()
        {
            if (_captureScheduled) return;
            _captureScheduled = true;
            StartCoroutine(CaptureAtEndOfFrame());
        }

        private IEnumerator CaptureAtEndOfFrame()
        {
            yield return null;
            _captureScheduled = false;
            CaptureSnapshot();
        }

        private void CaptureSnapshot()
        {
            _snapshot.Clear();
            _entitySnapshot.Clear();
            _destroyedEntitySnapshot = SaveManager.Instance != null
                ? SaveManager.Instance.CaptureDestroyedEntities()
                : new HashSet<string>();
            CheckpointEntity[] checkpointEntities = FindObjectsByType<CheckpointEntity>(FindObjectsInactive.Include);
            foreach (CheckpointEntity entity in checkpointEntities)
            {
                if (entity == null || string.IsNullOrWhiteSpace(entity.CheckpointId) || _entitySnapshot.ContainsKey(entity.CheckpointId)) continue;
                _entitySnapshot.Add(entity.CheckpointId, entity.Capture());
            }
            MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include);
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour == null || behaviour == this || behaviour is not ISaveable saveable) continue;
                if (string.IsNullOrWhiteSpace(saveable.SaveId) || _snapshot.ContainsKey(saveable.SaveId)) continue;
                try { _snapshot.Add(saveable.SaveId, saveable.SaveData()); }
                catch (Exception exception)
                {
                    Debug.LogWarning($"[Checkpoint] Could not capture '{saveable.SaveId}': {exception.Message}", behaviour);
                }
            }
            SnapshotCaptured?.Invoke();
        }

        private void RestoreSnapshot()
        {
            if (_snapshot.Count == 0 && _entitySnapshot.Count == 0) return;
            SaveManager.Instance?.RestoreDestroyedEntities(_destroyedEntitySnapshot);
            CheckpointEntity[] checkpointEntities = FindObjectsByType<CheckpointEntity>(FindObjectsInactive.Include);
            foreach (CheckpointEntity entity in checkpointEntities)
                if (entity != null && _entitySnapshot.TryGetValue(entity.CheckpointId, out CheckpointEntityState state))
                    entity.Restore(state);
            MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include);
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour == null || behaviour == this || behaviour is not ISaveable saveable) continue;
                if (!_snapshot.TryGetValue(saveable.SaveId, out string json)) continue;
                try { saveable.LoadData(json); }
                catch (Exception exception)
                {
                    Debug.LogWarning($"[Checkpoint] Could not restore '{saveable.SaveId}': {exception.Message}", behaviour);
                }
            }
        }

        public string SaveId => "SurvivalCheckpoint";
        public string SaveData() => JsonUtility.ToJson(new CheckpointSaveState { checkpointId = _current != null ? _current.CheckpointId : string.Empty });

        public void LoadData(string json)
        {
            CheckpointSaveState state = JsonUtility.FromJson<CheckpointSaveState>(json);
            _pendingCheckpointId = state?.checkpointId;
            SurvivalCheckpoint[] checkpoints = FindObjectsByType<SurvivalCheckpoint>();
            foreach (SurvivalCheckpoint checkpoint in checkpoints) Register(checkpoint);
        }
    }

}
