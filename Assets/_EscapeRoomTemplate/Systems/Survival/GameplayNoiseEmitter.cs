using UnityEngine;

namespace EscapeRoomRevolt.Systems.Survival
{
    public sealed class GameplayNoiseEmitter : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _radius = 7f;
        [SerializeField] private GameplayNoiseType _type = GameplayNoiseType.PlayerAction;

        public void Emit() => GameplayNoise.Emit(transform.position, _radius, _type, gameObject);

        public void Configure(float radius, GameplayNoiseType type)
        {
            _radius = Mathf.Max(0f, radius);
            _type = type;
        }
    }
}
