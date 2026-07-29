using System;
using System.Collections;
using Acuaria.Food;
using Acuaria.Simulation.Filtration;
using Acuaria.Simulation.Maintenance;
using Acuaria.UI.Aquarium;
using Acuaria.UI.WaterChemistry;
using UnityEngine;
using UnityEngine.UI;
using Acuaria.Aquarium.MultiAquarium;

namespace Acuaria.UI.Maintenance
{
    public sealed class AquariumMaintenanceController:MonoBehaviour
    {
        [SerializeField] AquariumMaintenanceDefinition definition;[SerializeField] FilterDefinition filterDefinition;
        [SerializeField] AquariumSimulationController simulation;[SerializeField] AquariumHUDController hud;
        [SerializeField] FeedingUIController feeding;[SerializeField] AquariumMaintenancePanel panel;
        [SerializeField] WaterChangeVisualController visuals;[SerializeField] Button openButton;[SerializeField] Button backButton;
        AquariumMaintenanceState state=new();FilterRuntimeState filterState=new();
        readonly WaterChangeModel waterChange=new();readonly FilterSimulationModel filterSimulation=new();
        readonly FilterMaintenanceModel filterMaintenance=new();int selected=25;bool applied;Coroutine flow;bool focused;
        public event Action<AquariumMaintenancePhase> PhaseChanged;public event Action<int> WaterChangeCompleted;
        public event Action<FilterMaintenanceType> FilterMaintenanceCompleted;public bool IsActive=>state.IsActive;
        public AquariumMaintenanceState State=>state;public FilterRuntimeState FilterState=>filterState;
        public AquariumInstance BoundAquarium { get; private set; }
        public void Bind(AquariumInstance aquarium)
        {
            if(aquarium==null||ReferenceEquals(BoundAquarium,aquarium))return;
            if(IsActive)Cancel();
            BoundAquarium=aquarium;state=aquarium.MaintenanceState;filterState=aquarium.EnsureFilterState(filterDefinition);Refresh();
        }
        public void Configure(AquariumMaintenanceDefinition maintenance,FilterDefinition filter,AquariumSimulationController chemistry,
            AquariumHUDController hudController,FeedingUIController feedingController,AquariumMaintenancePanel maintenancePanel,
            WaterChangeVisualController visual,Button open,Button back)
        {definition=maintenance;filterDefinition=filter;simulation=chemistry;hud=hudController;feeding=feedingController;panel=maintenancePanel;visuals=visual;openButton=open;backButton=back;}
        void Awake(){if(!state.IsInitialized)state.Initialize("starter-maintenance-state");if(filterDefinition!=null)filterState.Initialize("starter-filter-state",filterDefinition);selected=definition?.RecommendedPercentage??25;}
        void OnEnable(){openButton?.onClick.AddListener(Open);if(panel!=null){panel.PercentageSelected+=SelectIndex;panel.Confirmed+=Confirm;panel.Cancelled+=Cancel;
            panel.GentleRinseRequested+=GentleRinse;panel.DeepCleanRequested+=DeepClean;}simulation.ChemistryChanged+=OnChemistry;}
        void OnDisable(){openButton?.onClick.RemoveListener(Open);if(panel!=null){panel.PercentageSelected-=SelectIndex;panel.Confirmed-=Confirm;panel.Cancelled-=Cancel;
            panel.GentleRinseRequested-=GentleRinse;panel.DeepCleanRequested-=DeepClean;}if(simulation!=null)simulation.ChemistryChanged-=OnChemistry;if(flow!=null)StopCoroutine(flow);Unlock();visuals?.Restore();}
        void Update()=>state.AdvanceCooldown(Time.unscaledDeltaTime);
        public void SetAquariumFocused(bool value){focused=value;if(openButton!=null)openButton.gameObject.SetActive(value);if(!value&&!IsActive)panel?.Close();}
        public void Open(){if(!focused||IsActive)return;feeding?.SetInteractionEnabled(false);hud?.SetInteractionEnabled(false);panel?.Show();Refresh();}
        void SelectIndex(int index){if(definition==null||index<0||index>=definition.AllowedPercentages.Length)return;selected=definition.AllowedPercentages[index];Refresh();}
        void Confirm(){if(flow!=null||!state.Begin(selected,definition)){Refresh();return;}applied=false;flow=StartCoroutine(Run());}
        void Cancel(){if(IsActive)return;panel?.Close();feeding?.SetInteractionEnabled(true);hud?.SetInteractionEnabled(true);state.Cancel();}
        IEnumerator Run()
        {
            Lock();SetPhase(AquariumMaintenancePhase.Preparing);yield return new WaitForSecondsRealtime(0.25f);
            SetPhase(AquariumMaintenancePhase.Draining);yield return visuals.AnimateDrain(definition.DrainDuration,selected,p=>state.SetPhase(AquariumMaintenancePhase.Draining,p));
            ApplyOnce();SetPhase(AquariumMaintenancePhase.Refilling);yield return visuals.AnimateRefill(definition.RefillDuration,selected,p=>state.SetPhase(AquariumMaintenancePhase.Refilling,p));
            visuals.Restore();SetPhase(AquariumMaintenancePhase.Stabilizing);yield return new WaitForSecondsRealtime(definition.StabilizationDuration);
            state.Complete(definition.CooldownSeconds);SetPhase(AquariumMaintenancePhase.Completed);WaterChangeCompleted?.Invoke(selected);Refresh();
            yield return new WaitForSecondsRealtime(0.6f);flow=null;panel.Close();Unlock();state.ReturnToIdle();
        }
        void ApplyOnce(){if(applied)return;var current=simulation?.Snapshot;var result=waterChange.Calculate(current,definition,selected);if(!result.IsValid)return;
            applied=simulation.ApplyMaintenance(result.Ammonia,result.Nitrite,result.Nitrate,result.AmmoniaBacteria,result.NitriteBacteria,result.Waste);}
        void OnChemistry(Acuaria.Simulation.Water.WaterChemistryState chemistry){if(filterDefinition==null)return;
            var hours=chemistry.LastSimulationStep/3600f;filterSimulation.Step(filterState,filterDefinition,simulation.AquariumVolumeLiters,chemistry.OrganicWaste,hours);
            simulation.SetBiologicalEfficiency(1f+filterState.BiologicalCapacity);if(panel!=null&&panel.IsOpen)Refresh();}
        void GentleRinse()=>MaintainFilter(FilterMaintenanceType.GentleRinse);
        void DeepClean()=>MaintainFilter(FilterMaintenanceType.DeepClean);
        void MaintainFilter(FilterMaintenanceType type){if(IsActive||simulation?.Snapshot==null)return;
            var result=filterMaintenance.Calculate(filterState,type);if(!filterMaintenance.Apply(filterState,filterDefinition,type))return;
            var c=simulation.Snapshot;simulation.ApplyMaintenance(c.AmmoniaMgPerLiter,c.NitriteMgPerLiter,c.NitrateMgPerLiter,
                c.AmmoniaOxidizingBacteria*result.BacteriaRetention,c.NitriteOxidizingBacteria*result.BacteriaRetention,c.OrganicWaste);
            FilterMaintenanceCompleted?.Invoke(type);Refresh();}
        void SetPhase(AquariumMaintenancePhase phase){state.SetPhase(phase);PhaseChanged?.Invoke(phase);Refresh();}
        void Refresh(){if(panel==null||simulation?.Snapshot==null)return;var vm=new AquariumMaintenanceViewModel(definition,simulation.Snapshot,selected,filterState,state);
            panel.Render(vm.Preview,vm.FilterSummary,IsActive?PhaseLabel(state.Phase):vm.Cooldown);}
        void Lock(){panel?.SetBusy(true);feeding?.SetInteractionEnabled(false);if(openButton!=null)openButton.interactable=false;if(backButton!=null)backButton.interactable=false;hud?.SetInteractionEnabled(false);}
        void Unlock(){panel?.SetBusy(false);feeding?.SetInteractionEnabled(true);if(openButton!=null)openButton.interactable=focused;if(backButton!=null)backButton.interactable=true;hud?.SetInteractionEnabled(true);}
        static string PhaseLabel(AquariumMaintenancePhase phase)=>phase switch{AquariumMaintenancePhase.Preparing=>"Preparando",AquariumMaintenancePhase.Draining=>"Drenando agua",AquariumMaintenancePhase.Refilling=>"Llenando",AquariumMaintenancePhase.Stabilizing=>"Estabilizando el acuario",_=>phase.ToString()};
    }
}
