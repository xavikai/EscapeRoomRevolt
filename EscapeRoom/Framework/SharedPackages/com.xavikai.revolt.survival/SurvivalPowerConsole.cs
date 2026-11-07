using System;
using EscapeRoomRevolt.Systems.Interaction;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Survival
{
    [Serializable]
    internal sealed class PowerConsoleSaveState { public bool activated; }

    /// <summary>Demo-ready objective interaction that can unlock a shared framework Door.</summary>
    public sealed class SurvivalPowerConsole : InteractableBase
    {
        [SerializeField] private Door _controlledDoor;
        [SerializeField] private string _readyPrompt = "Restablir l'energia";
        [SerializeField] private string _completedPrompt = "Energia restablerta";
        [SerializeField, Min(0f)] private float _noiseRadius = 12f;
        private bool _activated;

        public bool IsActivated => _activated;
        public override string InteractionPrompt => _activated ? _completedPrompt : _readyPrompt;
        public override bool CanInteract => base.CanInteract && !_activated;

        protected override void OnInteract()
        {
            if (_activated) return;
            _activated = true;
            _controlledDoor?.Unlock();
            GameplayNoise.Emit(transform.position, _noiseRadius, GameplayNoiseType.PlayerAction, gameObject);
            SetInteractable(false);
        }

        public override string SaveData() => JsonUtility.ToJson(new PowerConsoleSaveState { activated = _activated });

        public override void LoadData(string json)
        {
            PowerConsoleSaveState state = JsonUtility.FromJson<PowerConsoleSaveState>(json);
            _activated = state != null && state.activated;
            if (_activated) _controlledDoor?.Unlock();
            SetInteractable(!_activated);
        }
    }
}
