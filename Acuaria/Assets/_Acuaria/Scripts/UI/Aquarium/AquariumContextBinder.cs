using Acuaria.Aquarium;
using Acuaria.Aquarium.Decorations;
using Acuaria.Aquarium.MultiAquarium;
using Acuaria.UI.Maintenance;
using Acuaria.UI.Progression;
using Acuaria.UI.WaterChemistry;
using UnityEngine;
using Acuaria.Fish;
using System.Collections.Generic;
using Acuaria.UI.FishWelfare;

namespace Acuaria.UI.Aquarium
{
    public sealed class AquariumContextBinder : MonoBehaviour
    {
        [SerializeField] AquariumManager manager;
        [SerializeField] AquariumDefinition starterDefinition;
        [SerializeField] AquariumHUDController hud;
        [SerializeField] AquariumSimulationController simulation;
        [SerializeField] AquariumMaintenanceController maintenance;
        [SerializeField] AquaristJournalController journal;
        [SerializeField] AquariumHabitatController habitat;
        [SerializeField] FishSpawner2D fishSpawner;
        [SerializeField] FishSpawner2D[] aquariumSpawners=System.Array.Empty<FishSpawner2D>();
        [SerializeField] FishWelfareController welfare;
        [SerializeField] bool manageFishPresentation=true;
        [SerializeField, Min(.25f)] float inactiveTickInterval = 5f;
        float elapsed;
        readonly List<FishRuntimeState> migrationBuffer=new();

        public AquariumManager Manager=>manager;
        public void Configure(AquariumManager source,AquariumDefinition definition,AquariumHUDController hudController,
            AquariumSimulationController simulationController,AquariumMaintenanceController maintenanceController,
            AquaristJournalController journalController,AquariumHabitatController habitatController)
        {manager=source;starterDefinition=definition;hud=hudController;simulation=simulationController;
         maintenance=maintenanceController;journal=journalController;habitat=habitatController;}
        public void SetFishSpawner(FishSpawner2D value)=>fishSpawner=value;
        public void SetManageFishPresentation(bool value)=>manageFishPresentation=value;
        public void SetAquariumSpawners(FishSpawner2D[] values)=>aquariumSpawners=values??System.Array.Empty<FishSpawner2D>();
        public void SetWelfareController(FishWelfareController value)=>welfare=value;

        void Start()
        {
            if(manager==null)manager=AquariumManager.Instance;
            if(manager==null){Debug.LogError("AquariumContextBinder requires AquariumManager.",this);return;}
            hud??=FindFirstObjectByType<AquariumHUDController>(FindObjectsInactive.Include);
            simulation??=FindFirstObjectByType<AquariumSimulationController>(FindObjectsInactive.Include);
            maintenance??=FindFirstObjectByType<AquariumMaintenanceController>(FindObjectsInactive.Include);
            journal??=FindFirstObjectByType<AquaristJournalController>(FindObjectsInactive.Include);
            habitat??=FindFirstObjectByType<AquariumHabitatController>(FindObjectsInactive.Include);
            welfare??=FindFirstObjectByType<FishWelfareController>(FindObjectsInactive.Include);
            manager.OnActiveAquariumChanged+=OnActiveChanged;
            if(manager.Aquariums.Count==0&&starterDefinition!=null)manager.CreateAquarium(starterDefinition,"aquarium-01","Acuario Inicial");
            if(fishSpawner==null)fishSpawner=FindFirstObjectByType<FishSpawner2D>();
            if(manager.ActiveAquarium!=null&&manager.ActiveAquarium.FishCollection.Count==0)
            {
                fishSpawner?.CopySpawnedStates(migrationBuffer);
                foreach(var state in migrationBuffer)manager.ActiveAquarium.FishCollection.Add(state);
            }
            Bind(manager.ActiveAquarium);
        }
        void OnDestroy(){if(manager!=null)manager.OnActiveAquariumChanged-=OnActiveChanged;}
        void Update()
        {
            if(manager==null)return;
            var delta=Time.unscaledDeltaTime;
            manager.ActiveAquarium?.StatisticsState.AddActiveTime(delta);
            elapsed+=delta;if(elapsed<inactiveTickInterval)return;
            manager.TickInactive(elapsed);elapsed=0;
        }
        void OnActiveChanged(AquariumInstance previous,AquariumInstance next)=>Bind(next);
        void Bind(AquariumInstance aquarium)
        {
            if(aquarium==null)return;
            aquarium.RuntimeState.SetFishCount(aquarium.FishCollection.Count);
            hud?.Bind(aquarium);simulation?.Bind(aquarium);maintenance?.Bind(aquarium);
            journal?.Bind(aquarium);habitat?.Bind(aquarium);
            var spawnerForAquarium=SpawnerFor(aquarium);
            welfare?.Bind(aquarium,spawnerForAquarium);
            if(manageFishPresentation)fishSpawner?.BindStates(aquarium.FishCollection.Fish);
        }
        FishSpawner2D SpawnerFor(AquariumInstance aquarium)
        {
            if(manager==null||aquariumSpawners==null)return fishSpawner;
            for(var index=0;index<manager.Aquariums.Count&&index<aquariumSpawners.Length;index++)
                if(ReferenceEquals(manager.Aquariums[index],aquarium))return aquariumSpawners[index];
            return fishSpawner;
        }
    }
}
