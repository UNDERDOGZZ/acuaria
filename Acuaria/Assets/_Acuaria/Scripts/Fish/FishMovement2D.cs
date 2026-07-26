using UnityEngine;

namespace Acuaria.Fish
{
    public sealed class FishMovement2D : MonoBehaviour
    {
        [SerializeField] private bool showGizmos;
        [SerializeField, Range(0.15f, 0.4f)] private float minimumHorizontalTravelFraction = 0.28f;
        [SerializeField, Min(0.01f)] private float targetArrivalThreshold = 0.2f;
        [SerializeField, Min(0.01f)] private float boundaryDetectionThreshold = 0.1f;
        [SerializeField, Min(0.1f)] private float stuckDetectionInterval = 1f;
        [SerializeField, Min(0.01f)] private float minimumHorizontalProgress = 0.08f;
        [SerializeField, Min(0.5f)] private float maximumStuckDuration = 3f;
        private AquariumSwimArea2D area;
        private FishSpeciesDefinition species;
        private FishRuntimeState state;
        private FishMovementModel2D model;
        private FishMovement2D[] neighbours;
        private bool hasPriorityTarget;
        private Vector2 priorityTarget;
        private float prioritySpeedMultiplier = 1f;
        private float welfareSpeedMultiplier = 1f;
        private float maintenanceVisualMultiplier = 1f;
        private bool explicitVisualPause;
        private float progressCheckTimer;
        private float timeWithoutHorizontalProgress;
        private float lastProgressX;

        public FishRuntimeState State => state;
        public Vector2 Direction => state?.Direction ?? Vector2.right;
        public FishSpeciesDefinition Species => species;
        public bool IsVisualMovementPaused => explicitVisualPause;
        public float FinalSpeedMultiplier => explicitVisualPause?0f:Mathf.Clamp(
            SafeMultiplier(welfareSpeedMultiplier)*SafeMultiplier(prioritySpeedMultiplier)*SafeMultiplier(maintenanceVisualMultiplier),
            FishMovementSpeedPolicy.MinimumActiveMultiplier,FishMovementSpeedPolicy.MaximumMultiplier);
        public void SetWelfareSpeedMultiplier(float value) => welfareSpeedMultiplier = ClampMultiplier(value,1f);
        public void SetMaintenanceVisualMultiplier(float value) => maintenanceVisualMultiplier = ClampMultiplier(value,1f);
        public void SetExplicitVisualPause(bool paused) => explicitVisualPause=paused;

        public void SetPriorityTarget(Vector2 target, float speedMultiplier)
        {
            hasPriorityTarget = true;
            priorityTarget = area != null ? area.NavigationBounds.Clamp(target) : target;
            prioritySpeedMultiplier = Mathf.Clamp(speedMultiplier, 1f, 1.3f);
            if (state != null) state.IsRecoveringFromBoundary = false;
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
            lastProgressX = state.Position.x;
            transform.localPosition = new Vector3(state.Position.x, state.Position.y, transform.localPosition.z);
        }

        private void Update()
        {
            if (state == null || model == null || area == null) return;
            var bounds = area.NavigationBounds;
            if (!hasPriorityTarget && FishMovementModel2D.NeedsBoundaryRecovery(state.Position, state.Direction,
                    state.Target, bounds, boundaryDetectionThreshold))
            {
                RecoverFromBoundary(bounds);
            }
            else if (!hasPriorityTarget && (FishMovementModel2D.TargetReached(state.Position, state.Target, bounds,
                         targetArrivalThreshold, boundaryDetectionThreshold) ||
                     state.TimeSinceTargetChange >= state.TargetDuration))
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

            var deltaTime=Time.unscaledDeltaTime;
            if(!float.IsFinite(deltaTime)||deltaTime<=0f||explicitVisualPause)return;
            var baseSpeed = state.CurrentSpeed;
            state.CurrentSpeed = FishMovementSpeedPolicy.Calculate(baseSpeed,welfareSpeedMultiplier,
                prioritySpeedMultiplier,maintenanceVisualMultiplier,false);
            model.Step(state, bounds, deltaTime);
            state.CurrentSpeed = baseSpeed;
            state.Position = area.ClampLocal(state.Position + correction * deltaTime);
            UpdateStuckDetection(deltaTime, bounds);
            var desired = new Vector3(state.Position.x, state.Position.y, transform.localPosition.z);
            transform.localPosition = Vector3.Lerp(transform.localPosition, desired, 1f - Mathf.Exp(-12f * deltaTime));
        }
        private static float ClampMultiplier(float value,float fallback)=>Mathf.Clamp(Safe(value,fallback),
            FishMovementSpeedPolicy.MinimumActiveMultiplier,FishMovementSpeedPolicy.MaximumMultiplier);
        private static float SafeMultiplier(float value)=>Safe(value,1f);
        private static float Safe(float value,float fallback)=>float.IsFinite(value)&&value>=0f?value:fallback;

        private void PickTarget()
        {
            var bounds = area.NavigationBounds;
            state.Target = model.ChooseWanderTarget(bounds, species.SwimmingLevel, species.VerticalPreference,
                state.Position, bounds.Width * minimumHorizontalTravelFraction);
            state.CurrentSpeed = model.ChooseSpeed(species.MinimumSpeed, species.MaximumSpeed);
            state.TargetDuration = model.ChooseDuration(species.MinimumTargetDuration, species.MaximumTargetDuration);
            state.TimeSinceTargetChange = 0f;
            state.IsRecoveringFromBoundary = false;
        }

        private void RecoverFromBoundary(SwimBounds2D bounds)
        {
            PickTarget();
            state.Direction = FishMovementModel2D.InteriorRecoveryDirection(state.Position, state.Target, bounds);
            state.IsRecoveringFromBoundary = true;
            ResetStuckDetection();
        }

        private void UpdateStuckDetection(float deltaTime, SwimBounds2D bounds)
        {
            if (hasPriorityTarget)
            {
                ResetStuckDetection();
                return;
            }

            progressCheckTimer += deltaTime;
            if (progressCheckTimer < stuckDetectionInterval) return;
            var elapsed = progressCheckTimer;
            progressCheckTimer = 0f;
            if (Mathf.Abs(state.Position.x - lastProgressX) < minimumHorizontalProgress)
                timeWithoutHorizontalProgress += elapsed;
            else
                timeWithoutHorizontalProgress = 0f;
            lastProgressX = state.Position.x;

            if (timeWithoutHorizontalProgress >= maximumStuckDuration)
                RecoverFromBoundary(bounds);
        }

        private void ResetStuckDetection()
        {
            progressCheckTimer = 0f;
            timeWithoutHorizontalProgress = 0f;
            lastProgressX = state?.Position.x ?? 0f;
        }

        private void OnDrawGizmosSelected()
        {
            if (!showGizmos || state == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, area.LocalToWorld(state.Target));
            Gizmos.DrawWireSphere(area.LocalToWorld(state.Target), 0.08f);
            Gizmos.color = state.IsRecoveringFromBoundary ? Color.red : Color.cyan;
            Gizmos.DrawRay(transform.position, area.transform.TransformVector(state.Direction * 0.5f));
        }
    }
}
