using Acuaria.Food;
using UnityEngine;

namespace Acuaria.Fish
{
    public sealed class FishView : MonoBehaviour
    {
        [SerializeField] private FishMovement2D movement;
        [SerializeField] private FishVisual2D visual;
        [SerializeField] private FishFeedingBehaviour feedingBehaviour;

        public void Initialize(AquariumSwimArea2D area, FishSpeciesDefinition species,
            FishRuntimeState state, FishMovement2D[] neighbours, float scale, AquariumFoodController foodController)
        {
            movement.Initialize(area, species, state, neighbours);
            visual.Initialize(species, state.RandomSeed, scale);
            feedingBehaviour?.Initialize(foodController, state);
        }

        public void Configure(FishMovement2D movementComponent, FishVisual2D visualComponent)
        {
            movement = movementComponent;
            visual = visualComponent;
        }

        public void ConfigureFeeding(FishFeedingBehaviour behaviour) => feedingBehaviour = behaviour;

        private void LateUpdate()
        {
            if (movement != null && visual != null) visual.Render(movement.Direction, Time.unscaledTime);
        }
    }
}
