using System;
using UnityEngine;

namespace Acuaria.Fish
{
    [Serializable]
    public sealed class FishRuntimeState
    {
        public string InstanceId { get; private set; }
        public string SpeciesId { get; private set; }
        public Vector2 Position { get; set; }
        public Vector2 Direction { get; set; }
        public float CurrentSpeed { get; set; }
        public Vector2 Target { get; set; }
        public int RandomSeed { get; private set; }
        public float TimeSinceTargetChange { get; set; }
        public float TargetDuration { get; set; }
        public bool IsRecoveringFromBoundary { get; set; }
        public bool IsInitialized { get; private set; }
        public float Satiety { get; set; }
        public float Hunger { get; private set; }
        public float Health { get; private set; } = 1f;
        public float Stress { get; private set; }
        public float Welfare { get; private set; } = 1f;

        public void Initialize(string instanceId, string speciesId, Vector2 position, int seed)
        {
            if (string.IsNullOrWhiteSpace(instanceId) || string.IsNullOrWhiteSpace(speciesId))
            {
                throw new ArgumentException("Fish instance and species IDs are required.");
            }

            InstanceId = instanceId;
            SpeciesId = speciesId;
            Position = position;
            Direction = Vector2.right;
            RandomSeed = seed;
            TimeSinceTargetChange = 0f;
            IsRecoveringFromBoundary = false;
            Satiety = 0.45f;
            Hunger = 0.55f; Health = 1f; Stress = 0f; Welfare = 1f;
            IsInitialized = true;
        }

        public void RestoreNeeds(float satiety, float hunger, float health, float stress, float welfare)
        {
            Satiety = Mathf.Clamp01(Safe(satiety, .45f));
            Hunger = Mathf.Clamp01(Safe(hunger, 1f - Satiety));
            Health = Mathf.Clamp01(Safe(health, 1f));
            Stress = Mathf.Clamp01(Safe(stress, 0f));
            Welfare = Mathf.Clamp01(Safe(welfare, 1f));
        }
        static float Safe(float value,float fallback)=>float.IsFinite(value)?value:fallback;
    }
}
