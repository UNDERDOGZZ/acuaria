using System;
using System.Collections.Generic;
using UnityEngine;

namespace Acuaria.Food
{
    public sealed class AquariumFoodController : MonoBehaviour
    {
        [SerializeField] private FoodDefinition definition;
        [SerializeField] private FoodView2D foodPrefab;
        [SerializeField, Range(8, 15)] private int maximumActiveUnits = 12;
        [SerializeField] private Vector2 localBoundsMin = new(-3.55f, -1.45f);
        [SerializeField] private Vector2 localBoundsMax = new(3.55f, 1.45f);
        [SerializeField] private AudioSource sharedAudioSource;
        [SerializeField] private AudioClip dropClip;
        [SerializeField] private AudioClip consumeClip;
        private readonly List<FoodView2D> activeFood = new(12);
        private int sequence;

        public event Action MaximumReached;
        public IReadOnlyList<FoodView2D> ActiveFood => activeFood;
        public int ActiveCount => activeFood.Count;
        public int MaximumActiveUnits => maximumActiveUnits;
        public float Bottom => localBoundsMin.y;

        public void Configure(FoodDefinition foodDefinition, FoodView2D prefab, int maximum,
            Vector2 boundsMin, Vector2 boundsMax)
        {
            definition = foodDefinition;
            foodPrefab = prefab;
            maximumActiveUnits = Mathf.Clamp(maximum, 8, 15);
            localBoundsMin = boundsMin;
            localBoundsMax = boundsMax;
        }

        public int SpawnPortion(Vector2 localSurfacePosition, int seed, int units = 3)
        {
            if (definition == null || !definition.IsValid || foodPrefab == null) return 0;
            var availableSlots = maximumActiveUnits - activeFood.Count;
            var spawnCount = Mathf.Clamp(units, 0, Mathf.Min(4, availableSlots));
            if (spawnCount == 0)
            {
                MaximumReached?.Invoke();
                return 0;
            }

            var random = new System.Random(seed);
            for (var index = 0; index < spawnCount; index++)
            {
                var offset = new Vector2(((float)random.NextDouble() - 0.5f) * 0.38f,
                    -((float)random.NextDouble() * 0.08f));
                var position = ClampLocal(localSurfacePosition + offset);
                var state = new FoodRuntimeState();
                state.Initialize($"food-{++sequence}", definition.FoodId, position,
                    definition.FallSpeed * Mathf.Lerp(0.88f, 1.12f, (float)random.NextDouble()),
                    definition.MaximumLifetime);
                var instance = Instantiate(foodPrefab, transform);
                var size = Mathf.Lerp(definition.MinimumVisualSize, definition.MaximumVisualSize,
                    (float)random.NextDouble());
                instance.Initialize(definition, state, this, size, (float)random.NextDouble() * 10f);
                activeFood.Add(instance);
            }
            if (sharedAudioSource != null && dropClip != null) sharedAudioSource.PlayOneShot(dropClip);
            return spawnCount;
        }

        public bool TryClaim(FoodView2D food, string fishId) =>
            food != null && activeFood.Contains(food) && food.State.TryClaim(fishId);

        public void Release(FoodView2D food, string fishId) => food?.State.Release(fishId);

        public bool Consume(FoodView2D food, string fishId)
        {
            if (food == null || !activeFood.Contains(food) || !food.State.Consume(fishId)) return false;
            activeFood.Remove(food);
            if (sharedAudioSource != null && consumeClip != null) sharedAudioSource.PlayOneShot(consumeClip);
            food.Consume();
            return true;
        }

        public void NotifyExpired(FoodView2D food) => activeFood.Remove(food);

        public Vector2 ClampLocal(Vector2 position) => new(
            Mathf.Clamp(position.x, localBoundsMin.x, localBoundsMax.x),
            Mathf.Clamp(position.y, localBoundsMin.y, localBoundsMax.y));

        private void OnDisable()
        {
            for (var index = activeFood.Count - 1; index >= 0; index--)
                if (activeFood[index] == null) activeFood.RemoveAt(index);
        }
    }
}
