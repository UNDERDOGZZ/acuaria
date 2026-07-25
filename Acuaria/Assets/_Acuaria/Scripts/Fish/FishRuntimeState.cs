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
        public bool IsInitialized { get; private set; }
        public float Satiety { get; set; }

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
            Satiety = 0.45f;
            IsInitialized = true;
        }
    }
}
