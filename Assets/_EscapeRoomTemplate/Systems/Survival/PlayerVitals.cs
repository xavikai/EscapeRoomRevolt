using System;
using System.Collections;
using EscapeRoomRevolt.Core.Flow;
using EscapeRoomRevolt.Core.Save;
using EscapeRoomRevolt.Core.Settings;
using EscapeRoomRevolt.Player.PC;
using EscapeRoomRevolt.Player.VR;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Survival
{
    [Serializable]
    public sealed class PlayerVitalsSaveState
    {
        public float health;
        public float stamina;
    }

    public enum DamageType { Generic, Enemy, Environment, Trap, Fall }

    public readonly struct DamageInfo
    {
        public readonly float Amount;
        public readonly DamageType Type;
        public readonly GameObject Source;
        public readonly Vector3 Point;

        public DamageInfo(float amount, DamageType type = DamageType.Generic, GameObject source = null,
            Vector3 point = default)
        {
            Amount = amount;
            Type = type;
            Source = source;
            Point = point;
        }

        public DamageInfo WithAmount(float amount) => new DamageInfo(amount, Type, Source, Point);
    }

    /// <summary>Survival-only health and sprint resource. Escape Room profiles disable it completely.</summary>
    [DefaultExecutionOrder(-35)]
    public sealed class PlayerVitals : MonoBehaviour, ISaveable
    {
        public static PlayerVitals Instance { get; private set; }

        [Header("Health")]
        [SerializeField, Min(1f)] private float _maxHealth = 100f;
        [SerializeField, Min(0f)] private float _damageInvulnerability = .35f;
        [SerializeField, Min(0f)] private float _deathRespawnDelay = 1.1f;
        [Header("Stamina")]
        [SerializeField, Min(1f)] private float _maxStamina = 100f;
        [SerializeField, Min(0f)] private float _sprintDrainPerSecond = 19f;
        [SerializeField, Min(0f)] private float _recoveryPerSecond = 14f;
        [SerializeField, Min(0f)] private float _recoveryDelay = 1.1f;
        [SerializeField, Range(0f, 1f)] private float _resumeSprintAt = .2f;

        private float _health;
        private float _stamina;
        private float _lastDrainTime;
        private bool _sprinting;
        private bool _exhausted;
        private bool _hidden;
        private bool _isDead;
        private float _nextDamageTime;
        private Coroutine _deathRoutine;

        public float Health => _health;
        public float MaxHealth => _maxHealth;
        public float Health01 => _maxHealth > 0f ? _health / _maxHealth : 0f;
        public float Stamina => _stamina;
        public float MaxStamina => _maxStamina;
        public float Stamina01 => _maxStamina > 0f ? _stamina / _maxStamina : 0f;
        public bool CanSprint => !_exhausted && _stamina > 0f && _health > 0f;
        public bool IsHidden => _hidden;
        public bool IsDead => _isDead;

        public event Action<float> HealthChanged;
        public event Action<float> StaminaChanged;
        public event Action<bool> ExhaustionChanged;
        public event Action<bool> HiddenChanged;
        public event Action Died;
        public event Action<float> Damaged;
        public event Action<DamageInfo> DamageReceived;
        public event Action<bool> DeathResolved;

        private void Awake()
        {
            if (!GameFeatures.IsEnabled(OptionalGameFeature.PlayerVitals)) { enabled = false; return; }
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            _health = _maxHealth;
            _stamina = _maxStamina;
            SaveManager.Instance?.Register(this);
        }

        private void Update()
        {
            if (_sprinting && CanSprint)
            {
                _stamina = Mathf.Max(0f, _stamina - _sprintDrainPerSecond * SurvivalDifficultyService.StaminaDrain * Time.deltaTime);
                _lastDrainTime = Time.time;
                if (_stamina <= 0f) SetExhausted(true);
                StaminaChanged?.Invoke(Stamina01);
            }
            else if (Time.time >= _lastDrainTime + _recoveryDelay && _stamina < _maxStamina)
            {
                _stamina = Mathf.Min(_maxStamina, _stamina + _recoveryPerSecond * SurvivalDifficultyService.StaminaRecovery * Time.deltaTime);
                if (_exhausted && Stamina01 >= _resumeSprintAt) SetExhausted(false);
                StaminaChanged?.Invoke(Stamina01);
            }
        }

        private void OnDestroy()
        {
            SaveManager.Instance?.Unregister(this);
            if (Instance == this) Instance = null;
        }

        public void SetSprinting(bool value) => _sprinting = value;

        public void SetHidden(bool value)
        {
            if (_hidden == value) return;
            _hidden = value;
            HiddenChanged?.Invoke(value);
        }

        public void ApplyDamage(float amount) => ApplyDamage(new DamageInfo(amount));

        public void ApplyDamage(DamageInfo damage)
        {
            float amount = damage.Amount * SurvivalDifficultyService.IncomingDamage;
            if (amount <= 0f || _health <= 0f || _isDead || Time.time < _nextDamageTime) return;
            _nextDamageTime = Time.time + _damageInvulnerability;
            _health = Mathf.Max(0f, _health - amount);
            Damaged?.Invoke(amount);
            DamageReceived?.Invoke(damage.WithAmount(amount));
            HealthChanged?.Invoke(Health01);
            if (_health > 0f) return;
            _sprinting = false;
            _isDead = true;
            SetControlBlocked(true);
            Died?.Invoke();
            if (_deathRoutine != null) StopCoroutine(_deathRoutine);
            _deathRoutine = StartCoroutine(ResolveDeath());
        }

        private IEnumerator ResolveDeath()
        {
            if (_deathRespawnDelay > 0f) yield return new WaitForSecondsRealtime(_deathRespawnDelay);
            bool respawned = CheckpointManager.Instance != null && CheckpointManager.Instance.TryRespawn(this);
            if (!respawned) GameFlowManager.EnsureInstance().FailGame();
            DeathResolved?.Invoke(respawned);
            _deathRoutine = null;
        }

        public void RestoreFull()
        {
            _health = _maxHealth;
            RestoreCommonState();
        }

        public void RestoreForCheckpoint()
        {
            _health = _maxHealth * SurvivalDifficultyService.CheckpointHealth;
            RestoreCommonState();
        }

        private void RestoreCommonState()
        {
            _stamina = _maxStamina;
            _isDead = false;
            _nextDamageTime = Time.time + _damageInvulnerability;
            SetExhausted(false);
            _sprinting = false;
            SetHidden(false);
            SetControlBlocked(false);
            HealthChanged?.Invoke(Health01);
            StaminaChanged?.Invoke(Stamina01);
        }

        public string SaveId => "PlayerVitals";
        public string SaveData() => JsonUtility.ToJson(new PlayerVitalsSaveState { health = _health, stamina = _stamina });

        public void LoadData(string json)
        {
            PlayerVitalsSaveState state = JsonUtility.FromJson<PlayerVitalsSaveState>(json);
            if (state == null) return;
            _health = Mathf.Clamp(state.health, 0f, _maxHealth);
            _stamina = Mathf.Clamp(state.stamina, 0f, _maxStamina);
            _isDead = false;
            SetExhausted(_stamina <= 0f);
            HealthChanged?.Invoke(Health01);
            StaminaChanged?.Invoke(Stamina01);
        }

        private void SetExhausted(bool value)
        {
            if (_exhausted == value) return;
            _exhausted = value;
            ExhaustionChanged?.Invoke(value);
        }

        private void SetControlBlocked(bool blocked)
        {
            PlayerMovement movement = GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.IsMovementFrozen = blocked;
                movement.IsMouseLookFrozen = blocked;
            }
            GetComponent<VRComfortController>()?.SetMovementBlocked(blocked);
        }
    }
}
