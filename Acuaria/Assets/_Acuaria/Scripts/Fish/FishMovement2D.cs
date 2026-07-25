using UnityEngine;

namespace Acuaria.Fish
{
    public sealed class FishMovement2D : MonoBehaviour
    {
        [SerializeField] private bool showGizmos;
        private AquariumSwimArea2D area;
        private FishSpeciesDefinition species;
        private FishRuntimeState state;
        private FishMovementModel2D model;
        private FishMovement2D[] neighbours;

        public FishRuntimeState State => state;
        public Vector2 Direction => state?.Direction ?? Vector2.right;

        public void Initialize(AquariumSwimArea2D swimArea, FishSpeciesDefinition definition,
            FishRuntimeState runtimeState, FishMovement2D[] nearbyFish)
        {
            area = swimArea;
            species = definition;
            state = runtimeState;
            neighbours = nearbyFish;
            model = new FishMovementModel2D(state.RandomSeed);
            PickTarget();
            transform.localPosition = new Vector3(state.Position.x, state.Position.y, transform.localPosition.z);
        }

        private void Update()
        {
            if (state == null || model == null || area == null) return;
            if ((state.Target - state.Position).sqrMagnitude < 0.04f ||
                state.TimeSinceTargetChange >= state.TargetDuration)
            {
                PickTarget();
            }

            var correction = Vector2.zero;
            if (neighbours != null)
            {
                for (var index = 0; index < neighbours.Length; index++)
                {
                    var other = neighbours[index];
                    if (other == null || other == this || other.state == null) continue;
                    correction += FishMovementModel2D.Separation(state.Position, other.state.Position, 0.75f, 0.22f);
                }
            }

            model.Step(state, area.LocalBounds, Time.deltaTime);
            state.Position = area.ClampLocal(state.Position + correction * Time.deltaTime);
            var desired = new Vector3(state.Position.x, state.Position.y, transform.localPosition.z);
            transform.localPosition = Vector3.Lerp(transform.localPosition, desired, 1f - Mathf.Exp(-12f * Time.deltaTime));
        }

        private void PickTarget()
        {
            state.Target = model.ChooseTarget(area.LocalBounds, species.SwimmingLevel, species.VerticalPreference);
            state.CurrentSpeed = model.ChooseSpeed(species.MinimumSpeed, species.MaximumSpeed);
            state.TargetDuration = model.ChooseDuration(species.MinimumTargetDuration, species.MaximumTargetDuration);
            state.TimeSinceTargetChange = 0f;
        }

        private void OnDrawGizmosSelected()
        {
            if (!showGizmos || state == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, area.LocalToWorld(state.Target));
            Gizmos.DrawWireSphere(area.LocalToWorld(state.Target), 0.08f);
        }
    }
}
