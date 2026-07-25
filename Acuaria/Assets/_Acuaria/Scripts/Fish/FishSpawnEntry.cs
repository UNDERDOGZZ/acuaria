using System;
using UnityEngine;

namespace Acuaria.Fish
{
    [Serializable]
    public struct FishSpawnEntry
    {
        [SerializeField] private FishSpeciesDefinition species;
        [SerializeField, Min(1)] private int quantity;
        [SerializeField] private FishView prefab;
        [SerializeField] private int baseSeed;

        public FishSpeciesDefinition Species => species;
        public int Quantity => quantity;
        public FishView Prefab => prefab;
        public int BaseSeed => baseSeed;

        public FishSpawnEntry(FishSpeciesDefinition definition, int count, FishView fishPrefab, int seed)
        {
            species = definition;
            quantity = count;
            prefab = fishPrefab;
            baseSeed = seed;
        }
    }
}
