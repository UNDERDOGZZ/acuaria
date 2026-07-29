using System;
using System.Collections.Generic;
using Acuaria.Food;
using UnityEngine;

namespace Acuaria.Fish
{
    public sealed class FishSpawner2D : MonoBehaviour
    {
        [SerializeField] private AquariumSwimArea2D swimArea;
        [SerializeField] private FishSpawnEntry[] entries;
        [SerializeField] private AquariumPopulationDefinition population;
        [SerializeField] private AquariumFoodController foodController;
        private readonly List<FishMovement2D> spawned = new(3);
        private readonly List<FishSpeciesDefinition> spawnedSpecies = new(3);
        private bool hasSpawned;

        public int SpawnedCount => spawned.Count;
        public event Action PopulationChanged;
        public IReadOnlyList<FishMovement2D> SpawnedFish => spawned;
        public AquariumSwimArea2D SwimArea => swimArea;

        private void Start() => Spawn();

        public void Spawn()
        {
            if (hasSpawned || swimArea == null) return;
            hasSpawned = true;
            var configuredCount=population!=null?population.TotalCount:LegacyCount();
            var pending = new List<(FishView view, FishSpeciesDefinition species, FishRuntimeState state, float scale)>(configuredCount);

            if(population!=null)
            {
                var populationEntries=population.Entries;
                if(populationEntries!=null)for(var entryIndex=0;entryIndex<populationEntries.Length;entryIndex++)
                {
                    var entry=populationEntries[entryIndex];if(entry?.IsValid!=true)continue;
                    var prefab=entry.Species.VisualDefinition?.Prefab;if(prefab==null)continue;
                    AddInstances(entry.Species,entry.Quantity,prefab,entry.BaseSeed,entryIndex,entry.InstanceNamePrefix,pending);
                }
            }
            else if(entries!=null)for(var entryIndex=0;entryIndex<entries.Length;entryIndex++)
            {var entry=entries[entryIndex];if(entry.Species==null||!entry.Species.IsValid||entry.Prefab==null)continue;
             AddInstances(entry.Species,entry.Quantity,entry.Prefab,entry.BaseSeed,entryIndex,null,pending);}

            var neighbours = spawned.ToArray();
            for (var index = 0; index < pending.Count; index++)
            {
                var fish = pending[index];
                fish.view.Initialize(swimArea, fish.species, fish.state, neighbours, fish.scale, foodController);
            }
            PopulationChanged?.Invoke();
        }

        void AddInstances(FishSpeciesDefinition species,int quantity,FishView prefab,int baseSeed,int entryIndex,string prefix,
            List<(FishView view,FishSpeciesDefinition species,FishRuntimeState state,float scale)> pending)
        {
            for(var count=0;count<quantity;count++){var seed=baseSeed+count*101+entryIndex*1009;var model=new FishMovementModel2D(seed);
             var position=model.ChooseTarget(swimArea.LocalBounds,species.SwimmingLevel,species.VerticalPreference);
             var instance=Instantiate(prefab,transform);var instanceId=$"{species.SpeciesId}-{entryIndex}-{count}";
             instance.name=string.IsNullOrWhiteSpace(prefix)?$"Fish_{species.SpeciesId}_{count+1}":$"{prefix}_{count+1}";
             var state=new FishRuntimeState();state.Initialize(instanceId,species.SpeciesId,position,seed);
             var scaleRange=species.VisualDefinition?.ScaleRange??new Vector2(species.MinimumScale,species.MaximumScale);
             var scale=model.ChooseSpeed(scaleRange.x,scaleRange.y);pending.Add((instance,species,state,scale));
             var movement=instance.GetComponent<FishMovement2D>();if(movement!=null)spawned.Add(movement);spawnedSpecies.Add(species);}
        }

        int LegacyCount(){var total=0;if(entries!=null)for(var i=0;i<entries.Length;i++)if(entries[i].Species!=null)total+=Mathf.Max(0,entries[i].Quantity);return total;}

        public void Configure(AquariumSwimArea2D area, FishSpawnEntry[] spawnEntries)
        {
            swimArea = area;
            entries = spawnEntries;
        }

        public void Configure(AquariumSwimArea2D area,AquariumPopulationDefinition populationDefinition)
        {swimArea=area;population=populationDefinition;}

        public void SetFoodController(AquariumFoodController controller) => foodController = controller;

        public void CopySpawnedSpecies(List<FishSpeciesDefinition> destination)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            destination.Clear();
            destination.AddRange(spawnedSpecies);
        }

        public void CopySpawnedStates(List<FishRuntimeState> destination)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            Spawn();
            destination.Clear();
            foreach (var movement in spawned)
                if (movement?.State != null) destination.Add(movement.State);
        }

        public void BindStates(IReadOnlyList<FishRuntimeState> states)
        {
            if (states == null) return;
            Spawn();
            var visibleCount = Mathf.Min(states.Count, spawned.Count);
            for (var i = 0; i < spawned.Count; i++)
            {
                var movement = spawned[i];
                if (movement == null) continue;
                var visible = i < visibleCount;
                movement.gameObject.SetActive(visible);
                if (visible && spawnedSpecies.Count > i)
                    movement.GetComponent<FishView>()?.Initialize(swimArea, spawnedSpecies[i], states[i],
                        spawned.ToArray(), movement.transform.localScale.x, foodController);
            }
        }
    }
}
