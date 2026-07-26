using Acuaria.Food;
using UnityEngine;

namespace Acuaria.Fish
{
    public sealed class FishFeedingBehaviour : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float maximumSatiety = 0.92f;
        [SerializeField, Min(0f)] private float satietyGainPerUnit = 0.22f;
        [SerializeField, Min(0f)] private float biteCooldown = 1.8f;
        [SerializeField] private FishMovement2D movement;
        [SerializeField] private FishVisual2D visual;
        private AquariumFoodController foodController;
        private FishRuntimeState runtimeState;
        private FoodView2D target;
        private bool hasTarget;
        private float cooldownRemaining;
        private float stateTimer;

        public FishBehaviourState State { get; private set; } = FishBehaviourState.Wandering;
        public FoodView2D Target => target;

        public void Configure(FishMovement2D movementComponent, FishVisual2D visualComponent)
        {
            movement = movementComponent;
            visual = visualComponent;
        }

        public void Initialize(AquariumFoodController controller, FishRuntimeState state)
        {
            foodController = controller;
            runtimeState = state;
            State = FishBehaviourState.Wandering;
        }

        private void Update()
        {
            if (runtimeState == null || movement == null || foodController == null) return;
            var deltaTime=Time.unscaledDeltaTime;
            cooldownRemaining = Mathf.Max(0f, cooldownRemaining - deltaTime);

            if (State == FishBehaviourState.Eating || State == FishBehaviourState.ResumingWander)
            {
                stateTimer -= deltaTime;
                if (stateTimer <= 0f)
                {
                    if (State == FishBehaviourState.Eating)
                    {
                        State = FishBehaviourState.ResumingWander;
                        stateTimer = 0.28f;
                    }
                    else State = FishBehaviourState.Wandering;
                }
                return;
            }

            if (hasTarget && target == null)
            {
                ClearTargetWithoutRelease();
            }
            else if (target != null && (target.State == null || target.State.IsTerminal ||
                                        target.State.ClaimedByFishId != runtimeState.InstanceId))
            {
                ReleaseTarget();
            }

            if (!hasTarget && cooldownRemaining <= 0f && runtimeState.Satiety < maximumSatiety)
                AcquireNearestFood();

            if (!hasTarget || target == null) return;
            movement.SetPriorityTarget(target.State.Position, 1.15f);
            var consumptionDistance = target.Definition.ConsumptionRadius;
            if ((runtimeState.Position - target.State.Position).sqrMagnitude <= consumptionDistance * consumptionDistance)
            {
                var consumed = foodController.Consume(target, runtimeState.InstanceId);
                if (consumed)
                {
                    runtimeState.Satiety = Mathf.Clamp01(runtimeState.Satiety + satietyGainPerUnit);
                    cooldownRemaining = biteCooldown;
                    State = FishBehaviourState.Eating;
                    stateTimer = 0.22f;
                    visual?.PlayEatingPulse();
                    target = null;
                    hasTarget = false;
                    movement.ClearPriorityTarget();
                }
                else ReleaseTarget();
            }
        }

        private void AcquireNearestFood()
        {
            FoodView2D nearest = null;
            var nearestDistance = float.PositiveInfinity;
            var foods = foodController.ActiveFood;
            for (var index = 0; index < foods.Count; index++)
            {
                var food = foods[index];
                if (food == null || food.State == null || food.State.IsTerminal) continue;
                if (food.State.State == FoodState.Claimed && food.State.ClaimedByFishId != runtimeState.InstanceId) continue;
                var distance = (runtimeState.Position - food.State.Position).sqrMagnitude;
                var radius = food.Definition.DetectionRadius;
                if (distance <= radius * radius && distance < nearestDistance)
                {
                    nearest = food;
                    nearestDistance = distance;
                }
            }

            if (nearest != null && foodController.TryClaim(nearest, runtimeState.InstanceId))
            {
                target = nearest;
                hasTarget = true;
                State = FishBehaviourState.SeekingFood;
            }
        }

        private void ReleaseTarget()
        {
            if (target != null && foodController != null && runtimeState != null)
                foodController.Release(target, runtimeState.InstanceId);
            ClearTargetWithoutRelease();
        }

        private void ClearTargetWithoutRelease()
        {
            target = null;
            hasTarget = false;
            movement?.ClearPriorityTarget();
            State = FishBehaviourState.Wandering;
        }

        private void OnDisable() => ReleaseTarget();
    }
}
