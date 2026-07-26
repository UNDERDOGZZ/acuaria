using System;
using UnityEngine;

namespace Acuaria.Fish
{
    public sealed class FishMovementModel2D
    {
        private readonly System.Random random;

        public FishMovementModel2D(int seed) => random = new System.Random(seed);

        public Vector2 ChooseTarget(SwimBounds2D bounds, SwimmingLevel level, float verticalPreference)
            => ChooseWanderTarget(bounds, level, verticalPreference, bounds.Center, 0f);

        public Vector2 ChooseWanderTarget(SwimBounds2D bounds, SwimmingLevel level, float verticalPreference,
            Vector2 currentPosition, float minimumHorizontalTravelDistance, int maximumAttempts = 6)
        {
            var zone = bounds.ForLevel(level);
            if (!zone.IsValid) return bounds.Clamp(currentPosition);

            var minimumTravel = Mathf.Clamp(SafeNonNegative(minimumHorizontalTravelDistance), 0f, zone.Width * 0.9f);
            var centerX = zone.Center.x;
            var chooseRight = currentPosition.x <= centerX;
            var sideMin = chooseRight ? centerX : zone.Left;
            var sideMax = chooseRight ? zone.Right : centerX;

            for (var attempt = 0; attempt < Mathf.Clamp(maximumAttempts, 1, 10); attempt++)
            {
                var x = Lerp(sideMin, sideMax, NextFloat());
                if (Mathf.Abs(x - currentPosition.x) < minimumTravel) continue;
                return new Vector2(x, ChooseVertical(zone, verticalPreference));
            }

            var fallbackX = chooseRight ? zone.Right - zone.Width * 0.05f : zone.Left + zone.Width * 0.05f;
            return zone.Clamp(new Vector2(fallbackX, ChooseVertical(zone, verticalPreference)));
        }

        public float ChooseSpeed(float minimum, float maximum) => Lerp(minimum, maximum, NextFloat());
        public float ChooseDuration(float minimum, float maximum) => Lerp(minimum, maximum, NextFloat());

        public Vector2 Step(FishRuntimeState state, SwimBounds2D bounds, float deltaTime)
        {
            var delta = state.Target - state.Position;
            if (TargetReached(state.Position, state.Target, bounds, 0.2f, 0.08f) ||
                state.TimeSinceTargetChange >= state.TargetDuration)
            {
                return state.Position;
            }

            var desired = delta.normalized;
            desired.y = Mathf.Clamp(desired.y, -0.42f, 0.42f);
            desired.Normalize();
            var blended = Vector2.Lerp(state.Direction, desired, Mathf.Clamp01(deltaTime * 2.8f)).normalized;
            blended.y = Mathf.Clamp(blended.y, -0.5f, 0.5f);
            state.Direction = blended;
            state.TimeSinceTargetChange += deltaTime;
            state.Position = bounds.Clamp(state.Position + state.Direction * (state.CurrentSpeed * deltaTime));
            return state.Position;
        }

        public static bool TargetReached(Vector2 position, Vector2 target, SwimBounds2D bounds,
            float arrivalThreshold, float boundaryThreshold)
        {
            var safeArrival = Mathf.Max(0.001f, SafeNonNegative(arrivalThreshold));
            if ((target - position).sqrMagnitude <= safeArrival * safeArrival) return true;

            var horizontalReached = Mathf.Abs(target.x - position.x) <= safeArrival;
            var nearHorizontalBoundary = position.x <= bounds.Left + SafeNonNegative(boundaryThreshold) ||
                                         position.x >= bounds.Right - SafeNonNegative(boundaryThreshold);
            return horizontalReached && nearHorizontalBoundary;
        }

        public static bool NeedsBoundaryRecovery(Vector2 position, Vector2 direction, Vector2 target,
            SwimBounds2D bounds, float boundaryThreshold)
        {
            var threshold = SafeNonNegative(boundaryThreshold);
            var nearLeft = position.x <= bounds.Left + threshold;
            var nearRight = position.x >= bounds.Right - threshold;
            return nearLeft && (direction.x < -0.01f || target.x <= position.x) ||
                   nearRight && (direction.x > 0.01f || target.x >= position.x);
        }

        public static Vector2 InteriorRecoveryDirection(Vector2 position, Vector2 target, SwimBounds2D bounds)
        {
            var inwardSign = position.x >= bounds.Center.x ? -1f : 1f;
            var vertical = Mathf.Clamp(target.y - position.y, -0.35f, 0.35f);
            return new Vector2(inwardSign, vertical).normalized;
        }

        public static Vector2 Separation(Vector2 position, Vector2 other, float radius, float strength)
        {
            var delta = position - other;
            var distanceSquared = delta.sqrMagnitude;
            if (distanceSquared <= 0.000001f || distanceSquared >= radius * radius) return Vector2.zero;
            var distance = Mathf.Sqrt(distanceSquared);
            return delta / distance * ((1f - distance / radius) * strength);
        }

        private float NextFloat() => (float)random.NextDouble();
        private float ChooseVertical(SwimBounds2D zone, float verticalPreference)
        {
            var biased = Mathf.Clamp01(NextFloat() + verticalPreference * 0.18f);
            return Lerp(zone.Bottom, zone.Top, biased);
        }

        private static float SafeNonNegative(float value) => float.IsFinite(value) ? Mathf.Max(0f, value) : 0f;
        private static float Lerp(float a, float b, float t) => a + (b - a) * t;
    }
}
