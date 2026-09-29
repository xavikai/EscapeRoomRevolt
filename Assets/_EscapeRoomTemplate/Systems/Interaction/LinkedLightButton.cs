using EscapeRoomRevolt.Systems.Puzzle;
using UnityEngine;

namespace EscapeRoomRevolt.Systems.Interaction
{
    public sealed class LinkedLightButton : InteractableBase
    {
        [SerializeField] private LinkedLightsPuzzle _puzzle;
        [SerializeField] private int _index;
        public override bool CanInteract => base.CanInteract && _puzzle != null && !_puzzle.IsSolved;
        public override string InteractionPrompt =>
            $"{EscapeRoomRevolt.Core.Localization.LocalizationService.Tr("Conmutar luz")} {_index + 1} · {EscapeRoomRevolt.Core.Localization.LocalizationService.Tr(_puzzle != null && _puzzle.IsLightOn(_index) ? "encendida" : "apagada")}";
        protected override void OnInteract() => _puzzle.Press(_index);
    }
}
