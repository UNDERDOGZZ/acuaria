using System;
using System.Collections.Generic;
using Acuaria.Food;
using Acuaria.Simulation.Time;
using Acuaria.Simulation.Waste;
using Acuaria.Simulation.Water;
using Acuaria.UI.Aquarium;
using Acuaria.Aquarium.MultiAquarium;
using UnityEngine;

namespace Acuaria.UI.WaterChemistry
{
    public sealed class AquariumSimulationController : MonoBehaviour
    {
        [SerializeField] private WaterChemistryDefinition definition;
        [SerializeField] private AquariumInhabitantProvider inhabitants;
        [SerializeField] private AquariumFoodController foodController;
        [SerializeField] private AquariumHUDController hud;
        [SerializeField] private float aquariumVolumeLiters = 50f;
        private readonly HashSet<string> processedExpiredFood = new();
        private readonly NitrogenCycleModel nitrogenCycle = new();
        private AquariumSimulationClock clock;
        private WaterChemistryState state;
        private AquariumCycleStatus lastCycle;
        private WaterQualityStatus lastQuality;
        private float biologicalEfficiency = 1f;
        private AquariumInstance boundAquarium;

        public event Action<WaterChemistryState> ChemistryChanged;
        public event Action<WaterQualityStatus> WaterQualityChanged;
        public event Action<AquariumCycleStatus> CycleStatusChanged;
        public event Action<float> WasteAdded;
        public WaterChemistryState Snapshot => state?.Snapshot();
        public WaterChemistryDefinition Definition => definition;
        public float AquariumVolumeLiters => aquariumVolumeLiters;
        public AquariumInstance BoundAquarium => boundAquarium;

        public void Bind(AquariumInstance aquarium)
        {
            if (aquarium == null || ReferenceEquals(boundAquarium, aquarium)) return;
            boundAquarium = aquarium;
            aquariumVolumeLiters = Mathf.Max(1f, aquarium.Definition.NominalVolumeLitres);
            if (aquarium.WaterState == null || !aquarium.WaterState.IsInitialized)
            {
                var created = new WaterChemistryState();
                if (definition != null) created.Initialize($"{aquarium.InstanceId}-water", definition);
                aquarium.ReplaceWaterState(created);
            }
            state = aquarium.WaterState;
            clock = definition == null ? null : new AquariumSimulationClock(definition.SimulationIntervalSeconds,
                definition.TimeMultiplier, definition.MaximumTicksPerFrame);
            processedExpiredFood.Clear();
            Publish(true);
        }

        public void Configure(WaterChemistryDefinition chemistryDefinition, AquariumInhabitantProvider provider,
            AquariumFoodController food, AquariumHUDController hudController, float volumeLiters)
        {
            definition = chemistryDefinition;
            inhabitants = provider;
            foodController = food;
            hud = hudController;
            aquariumVolumeLiters = Mathf.Max(1f, volumeLiters);
        }

        private void Awake() => Initialize();
        private void OnEnable()
        {
            if (state == null) Initialize();
            if (foodController != null)
            {
                foodController.FoodExpired += HandleFoodExpired;
                foodController.FoodConsumed += HandleFoodConsumed;
            }
        }
        private void OnDisable()
        {
            if (foodController != null)
            {
                foodController.FoodExpired -= HandleFoodExpired;
                foodController.FoodConsumed -= HandleFoodConsumed;
            }
        }
        private void Update() => clock?.Advance(Time.unscaledDeltaTime, SimulateTick);

        public void Initialize()
        {
            if (definition == null) return;
            state = new WaterChemistryState();
            state.Initialize("starter-water-state", definition);
            clock = new AquariumSimulationClock(definition.SimulationIntervalSeconds,
                definition.TimeMultiplier, definition.MaximumTicksPerFrame);
            Publish(true);
        }

        public void SimulateTick(float simulatedSeconds)
        {
            if (state == null || definition == null) return;
            var fishCount=boundAquarium?.FishCollection.Count??inhabitants?.TotalCount??0;
            var fishWaste = AquariumWasteModel.FishWaste(fishCount,
                simulatedSeconds / 3600f, definition);
            AddWaste(fishWaste);
            state = nitrogenCycle.Step(state, definition, simulatedSeconds, biologicalEfficiency);
            if (boundAquarium != null) boundAquarium.ReplaceWaterState(state);
            Publish(false);
        }

        public void AddWaste(float amount)
        {
            if (state == null || definition == null || amount <= 0f || !float.IsFinite(amount)) return;
            state.AddWaste(amount, definition);
            WasteAdded?.Invoke(amount);
        }

        public void AddAmmoniaDebug(float amount)
        {
            state?.AddAmmonia(amount, definition);
            Publish(false);
        }
        public void ResetChemistryDebug() => Initialize();
        public void SetBacteriaDebug(float level)
        {
            if (state == null) return;
            state.SetDevelopmentValues(state.AmmoniaMgPerLiter, state.NitriteMgPerLiter,
                state.NitrateMgPerLiter, level, level, state.OrganicWaste, definition);
            Publish(false);
        }
        public void SetPaused(bool paused)
        {
            if (paused) clock?.Pause();
            else clock?.Resume();
        }

        public void SetBiologicalEfficiency(float efficiency) =>
            biologicalEfficiency = float.IsFinite(efficiency) ? Mathf.Clamp(efficiency, 0f, 2f) : 0f;

        public bool ApplyMaintenance(float ammonia, float nitrite, float nitrate, float ammoniaBacteria,
            float nitriteBacteria, float waste)
        {
            if (state == null || definition == null) return false;
            state.ApplyMaintenanceValues(ammonia, nitrite, nitrate, ammoniaBacteria, nitriteBacteria, waste, definition);
            Publish(true);
            return true;
        }

        private void HandleFoodExpired(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !processedExpiredFood.Add(id)) return;
            AddWaste(AquariumWasteModel.ExpiredFoodWaste(1, definition));
        }
        private void HandleFoodConsumed(string id) =>
            AddWaste(AquariumWasteModel.ConsumedFoodWaste(1, definition));

        private void Publish(bool force)
        {
            if (state == null || definition == null) return;
            var cycle = AquariumCycleEvaluator.Evaluate(state, definition);
            var quality = WaterQualityEvaluator.Evaluate(state, definition, cycle);
            ChemistryChanged?.Invoke(state.Snapshot());
            if (force || cycle != lastCycle)
            {
                lastCycle = cycle;
                CycleStatusChanged?.Invoke(cycle);
            }
            if (force || quality.Status != lastQuality)
            {
                lastQuality = quality.Status;
                WaterQualityChanged?.Invoke(quality.Status);
            }
            hud?.SetWaterChemistry(new WaterChemistryViewModel(state, definition));
        }
    }
}
