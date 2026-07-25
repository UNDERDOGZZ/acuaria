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
        private bool hasPriorityTarget;
        private Vector2 priorityTarget;
        private float prioritySpeedMultiplier = 1f;
        private float welfareSpeedMultiplier = 1f;

        public FishRuntimeState State => state;
        public Vector2 Direction => state?.Direction ?? Vector2.right;
        public FishSpeciesDefinition Species => species;
        public void SetWelfareSpeedMultiplier(float value) => welfareSpeedMultiplier = Mathf.Clamp(value, 0.5f, 1f);

        public void SetPriorityTarget(Vector2 target, float speedMultiplier)
        {
            hasPriorityTarget = true;
            priorityTarget = area != null ? area.ClampLocal(target) : target;
            prioritySpeedMultiplier = Mathf.Clamp(speedMultiplier, 1f, 1.3f);
        }

        public void ClearPriorityTarget()
        {
            hasPriorityTarget = false;
            prioritySpeedMultiplier = 1f;
            if (state != null) PickTarget();
        }

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
            if (!hasPriorityTarget && ((state.Target - state.Position).sqrMagnitude < 0.04f ||
                state.TimeSinceTargetChange >= state.TargetDuration)
               )
            {
                PickTarget();
            }
            if (hasPriorityTarget) state.Target = priorityTarget;

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

            var baseSpeed = state.CurrentSpeed;
            state.CurrentSpeed = baseSpeed * prioritySpeedMultiplier * welfareSpeedMultiplier;
            model.Step(state, area.LocalBounds, Time.deltaTime);
            state.CurrentSpeed = baseSpeed;
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
