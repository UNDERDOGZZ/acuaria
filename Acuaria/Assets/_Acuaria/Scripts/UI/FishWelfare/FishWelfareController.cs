using System;
using System.Collections.Generic;
using Acuaria.Fish;
using Acuaria.Fish.Care;
using Acuaria.Fish.Compatibility;
using Acuaria.Fish.Welfare;
using Acuaria.Simulation.Water;
using Acuaria.UI.Aquarium;
using Acuaria.UI.WaterChemistry;
using Acuaria.Aquarium;
using Acuaria.Aquarium.Decorations;
using UnityEngine;
namespace Acuaria.UI.FishWelfare
{
    public sealed class FishWelfareController:MonoBehaviour
    {
        [SerializeField] FishWelfareDefinition definition;[SerializeField] FishSpawner2D spawner;
        [SerializeField] AquariumInhabitantProvider inhabitants;[SerializeField] AquariumSimulationController simulation;
        [SerializeField] AquariumHUDController hud;[SerializeField] float aquariumVolume=50;
        [SerializeField] AquariumDefinition aquariumDefinition;
        [SerializeField] AquariumHabitatController habitatController;
        readonly Dictionary<string,FishWelfareState> stateById=new();readonly List<FishWelfareState> orderedStates=new();
        readonly List<FishSpeciesDefinition> uniqueSpecies=new();readonly FishWelfareEvaluator evaluator=new();
        readonly FishWelfareSimulationModel evolution=new();readonly AquariumStockingModel stocking=new();
        readonly AquariumHabitatProfile fallbackHabitat=new();float accumulator;
        public event Action<AquariumWelfareResult> AquariumWelfareChanged;public event Action<string,FishWelfareState> FishWelfareChanged;
        public AquariumWelfareResult Current{get;private set;}public IReadOnlyList<FishWelfareState> States=>orderedStates;
        public void Configure(FishWelfareDefinition settings,FishSpawner2D fishSpawner,AquariumInhabitantProvider provider,
            AquariumSimulationController chemistry,AquariumHUDController hudController,float volume)
        {definition=settings;spawner=fishSpawner;inhabitants=provider;simulation=chemistry;hud=hudController;aquariumVolume=Mathf.Max(1,volume);}
        void OnEnable(){if(inhabitants!=null)inhabitants.PopulationChanged+=Rebuild;if(simulation!=null)simulation.ChemistryChanged+=OnChemistry;Rebuild();}
        void OnDisable(){if(inhabitants!=null)inhabitants.PopulationChanged-=Rebuild;if(simulation!=null)simulation.ChemistryChanged-=OnChemistry;}
        void Update(){accumulator+=Time.unscaledDeltaTime;if(accumulator<1f)return;var elapsed=accumulator;accumulator=0;Evaluate(elapsed/3600f);}
        void OnChemistry(WaterChemistryState state)=>Evaluate(Mathf.Max(.001f,state.LastSimulationStep/3600f));
        void Rebuild(){stateById.Clear();orderedStates.Clear();Evaluate(0);}
        public void Evaluate(float hours)
        {
            if(definition==null||spawner==null||simulation?.Snapshot==null)return;var fish=spawner.SpawnedFish;uniqueSpecies.Clear();
            for(var i=0;i<fish.Count;i++)if(fish[i]?.Species!=null&&!uniqueSpecies.Contains(fish[i].Species))uniqueSpecies.Add(fish[i].Species);
            var stock=stocking.Evaluate(GetAllSpecies(fish),aquariumVolume);var report=new AquariumCompatibilityReport(uniqueSpecies,aquariumVolume);
            orderedStates.Clear();for(var i=0;i<fish.Count;i++){var movement=fish[i];if(movement?.State==null||movement.Species==null)continue;
                if(!stateById.TryGetValue(movement.State.InstanceId,out var state)){state=new FishWelfareState();state.Initialize(movement.State.InstanceId);stateById.Add(movement.State.InstanceId,state);}
                var same=0;for(var j=0;j<fish.Count;j++)if(fish[j]?.Species?.SpeciesId==movement.Species.SpeciesId)same++;
                var compatibility=WorstFor(movement.Species,uniqueSpecies,aquariumVolume);var snapshot=simulation.Snapshot;
                var cycle=AquariumCycleEvaluator.Evaluate(snapshot,simulation.Definition);var quality=WaterQualityEvaluator.Evaluate(snapshot,simulation.Definition,cycle);
                var habitat=habitatController!=null?habitatController.CurrentProfile:aquariumDefinition!=null?aquariumDefinition.HabitatProfile:fallbackHabitat;
                var context=new FishWelfareContext(hud?.RuntimeState.CurrentTemperature??25,aquariumVolume,movement.State.Satiety,same,quality.Status,compatibility,habitat,stock.Status);
                var result=evaluator.Evaluate(movement.Species,context,definition);evolution.Step(state,result,definition,Mathf.Max(0,hours));
                movement.SetWelfareSpeedMultiplier(FishWelfareVisualAdapter.SpeedMultiplier(state.Status));orderedStates.Add(state);FishWelfareChanged?.Invoke(state.FishInstanceId,state);}
            Current=AquariumWelfareEvaluator.Evaluate(orderedStates);var vm=new FishWelfareViewModel(Current,fish,orderedStates,report);
            hud?.SetFishWelfare(vm.CompactText,vm.DetailsText+"\n"+vm.CompatibilityText,Current.Status);AquariumWelfareChanged?.Invoke(Current);
        }
        static List<FishSpeciesDefinition> GetAllSpecies(IReadOnlyList<FishMovement2D> fish){var list=new List<FishSpeciesDefinition>(fish.Count);for(var i=0;i<fish.Count;i++)if(fish[i]?.Species!=null)list.Add(fish[i].Species);return list;}
        static FishCompatibilityStatus WorstFor(FishSpeciesDefinition source,IReadOnlyList<FishSpeciesDefinition> species,float volume)
        {var worst=FishCompatibilityStatus.Compatible;var evaluator=new FishCompatibilityEvaluator();for(var i=0;i<species.Count;i++){if(species[i]==source)continue;var status=evaluator.Evaluate(source,species[i],volume).Status;if(status>worst)worst=status;}return worst;}
    }
}
