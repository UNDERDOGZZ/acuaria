using System;
using UnityEngine;

namespace Acuaria.Fish
{
    public sealed class FishMovementModel2D
    {
        private readonly System.Random random;

        public FishMovementModel2D(int seed) => random = new System.Random(seed);

        public Vector2 ChooseTarget(SwimBounds2D bounds, SwimmingLevel level, float verticalPreference)
        {
            var zone = bounds.ForLevel(level);
            var x = Lerp(zone.Left, zone.Right, NextFloat());
            var biased = Mathf.Clamp01(NextFloat() + verticalPreference * 0.18f);
            return new Vector2(x, Lerp(zone.Bottom, zone.Top, biased));
        }

        public float ChooseSpeed(float minimum, float maximum) => Lerp(minimum, maximum, NextFloat());
        public float ChooseDuration(float minimum, float maximum) => Lerp(minimum, maximum, NextFloat());

        public Vector2 Step(FishRuntimeState state, SwimBounds2D bounds, float deltaTime)
        {
            var delta = state.Target - state.Position;
            if (delta.sqrMagnitude < 0.04f || state.TimeSinceTargetChange >= state.TargetDuration)
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

        public static Vector2 Separation(Vector2 position, Vector2 other, float radius, float strength)
        {
            var delta = position - other;
            var distanceSquared = delta.sqrMagnitude;
            if (distanceSquared <= 0.000001f || distanceSquared >= radius * radius) return Vector2.zero;
            var distance = Mathf.Sqrt(distanceSquared);
            return delta / distance * ((1f - distance / radius) * strength);
        }

        private float NextFloat() => (float)random.NextDouble();
        private static float Lerp(float a, float b, float t) => a + (b - a) * t;
    }
}
