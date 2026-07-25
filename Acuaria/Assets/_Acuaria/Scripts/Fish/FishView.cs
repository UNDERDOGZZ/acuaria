using UnityEngine;

namespace Acuaria.Fish
{
    public sealed class FishView : MonoBehaviour
    {
        [SerializeField] private FishMovement2D movement;
        [SerializeField] private FishVisual2D visual;

        public void Initialize(AquariumSwimArea2D area, FishSpeciesDefinition species,
            FishRuntimeState state, FishMovement2D[] neighbours, float scale)
        {
            movement.Initialize(area, species, state, neighbours);
            visual.Initialize(species, state.RandomSeed, scale);
        }

        public void Configure(FishMovement2D movementComponent, FishVisual2D visualComponent)
        {
            movement = movementComponent;
            visual = visualComponent;
        }

        private void LateUpdate()
        {
            if (movement != null && visual != null) visual.Render(movement.Direction, Time.time);
        }
    }
}
