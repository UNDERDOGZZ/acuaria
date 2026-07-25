using System.Collections.Generic;
using Acuaria.Food;
using UnityEngine;

namespace Acuaria.Fish
{
    public sealed class FishSpawner2D : MonoBehaviour
    {
        [SerializeField] private AquariumSwimArea2D swimArea;
        [SerializeField] private FishSpawnEntry[] entries;
        [SerializeField] private AquariumFoodController foodController;
        private readonly List<FishMovement2D> spawned = new(3);
        private bool hasSpawned;

        public int SpawnedCount => spawned.Count;

        private void Start() => Spawn();

        public void Spawn()
        {
            if (hasSpawned || swimArea == null || entries == null) return;
            hasSpawned = true;
            var pending = new List<(FishView view, FishSpeciesDefinition species, FishRuntimeState state, float scale)>(3);

            for (var entryIndex = 0; entryIndex < entries.Length; entryIndex++)
            {
                var entry = entries[entryIndex];
                if (entry.Species == null || !entry.Species.IsValid || entry.Prefab == null) continue;
                for (var count = 0; count < entry.Quantity && pending.Count < 3; count++)
                {
                    var seed = entry.BaseSeed + count * 101 + entryIndex * 1009;
                    var model = new FishMovementModel2D(seed);
                    var position = model.ChooseTarget(swimArea.LocalBounds, entry.Species.SwimmingLevel,
                        entry.Species.VerticalPreference);
                    var instance = Instantiate(entry.Prefab, transform);
                    instance.name = $"Fish_{entry.Species.SpeciesId}_{count + 1}";
                    var state = new FishRuntimeState();
                    state.Initialize($"{entry.Species.SpeciesId}-{entryIndex}-{count}", entry.Species.SpeciesId,
                        position, seed);
                    var scale = model.ChooseSpeed(entry.Species.MinimumScale, entry.Species.MaximumScale);
                    pending.Add((instance, entry.Species, state, scale));
                    spawned.Add(instance.GetComponent<FishMovement2D>());
                }
            }

            var neighbours = spawned.ToArray();
            for (var index = 0; index < pending.Count; index++)
            {
                var fish = pending[index];
                fish.view.Initialize(swimArea, fish.species, fish.state, neighbours, fish.scale, foodController);
            }
        }

        public void Configure(AquariumSwimArea2D area, FishSpawnEntry[] spawnEntries)
        {
            swimArea = area;
            entries = spawnEntries;
        }

        public void SetFoodController(AquariumFoodController controller) => foodController = controller;
    }
}
