using System;

namespace Acuaria.Simulation.Time
{
    public sealed class AquariumSimulationClock
    {
        private double accumulator;
        public float IntervalSeconds { get; private set; }
        public float TimeMultiplier { get; private set; }
        public int MaximumTicksPerFrame { get; private set; }
        public bool IsPaused { get; private set; }

        public AquariumSimulationClock(float intervalSeconds, float multiplier, int maximumTicks)
        {
            Configure(intervalSeconds, multiplier, maximumTicks);
        }

        public void Configure(float intervalSeconds, float multiplier, int maximumTicks)
        {
            IntervalSeconds = float.IsFinite(intervalSeconds) && intervalSeconds > 0f ? intervalSeconds : 1f;
            TimeMultiplier = float.IsFinite(multiplier) && multiplier > 0f ? multiplier : 1f;
            MaximumTicksPerFrame = Math.Clamp(maximumTicks, 1, 20);
        }

        public int Advance(float realDeltaSeconds, Action<float> tick)
        {
            if (IsPaused || tick == null || !float.IsFinite(realDeltaSeconds) || realDeltaSeconds <= 0f) return 0;
            accumulator += realDeltaSeconds;
            var count = 0;
            while (accumulator >= IntervalSeconds && count < MaximumTicksPerFrame)
            {
                accumulator -= IntervalSeconds;
                tick(IntervalSeconds * TimeMultiplier);
                count++;
            }
            if (count == MaximumTicksPerFrame && accumulator > IntervalSeconds * MaximumTicksPerFrame)
                accumulator = IntervalSeconds * MaximumTicksPerFrame;
            return count;
        }

        public void Pause() => IsPaused = true;
        public void Resume() => IsPaused = false;
        public void Reset() => accumulator = 0d;
    }
}
