using System;
using System.Collections.Generic;
using Acuaria.Aquarium.Decorations;
using Acuaria.Fish.Care;
using Acuaria.Fish;
using Acuaria.Simulation.Maintenance;
using Acuaria.Simulation.Water;
using Acuaria.Simulation.Filtration;

namespace Acuaria.Aquarium.MultiAquarium
{
    [Serializable]
    public sealed class AquariumFishCollection
    {
        readonly List<FishRuntimeState> fish = new();
        public IReadOnlyList<FishRuntimeState> Fish => fish;
        public int Count => fish.Count;
        public event Action Changed;
        public bool Add(FishRuntimeState state)
        {
            if (state?.IsInitialized != true || Contains(state.InstanceId)) return false;
            fish.Add(state); Changed?.Invoke(); return true;
        }
        public bool Add(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || Contains(id)) return false;
            var state = new FishRuntimeState();
            state.Initialize(id, "unknown", UnityEngine.Vector2.zero, id.GetHashCode());
            return Add(state);
        }
        public bool Remove(string id)
        {
            var index = fish.FindIndex(item => item.InstanceId == id);
            if (index < 0) return false;
            fish.RemoveAt(index); Changed?.Invoke(); return true;
        }
        public bool Contains(string id) => fish.Exists(item => item.InstanceId == id);
        public void Replace(IEnumerable<FishRuntimeState> values)
        {
            fish.Clear();
            if (values != null)
                foreach (var value in values)
                    if (value?.IsInitialized == true && !Contains(value.InstanceId)) fish.Add(value);
            Changed?.Invoke();
        }
    }

    [Serializable]
    public sealed class AquariumDecorationCollection
    {
        readonly List<DecorationPlacementData> placements = new();
        public IReadOnlyList<DecorationPlacementData> Placements => placements;
        public void Replace(IEnumerable<DecorationPlacementData> values) { placements.Clear(); if (values != null) placements.AddRange(values); }
    }

    [Serializable]
    public sealed class AquariumJournalState
    {
        readonly List<string> entries = new();
        public IReadOnlyList<string> Entries => entries;
        public void Record(string entry) { if (!string.IsNullOrWhiteSpace(entry)) entries.Add(entry); }
        public void Restore(IEnumerable<string> values)
        {
            entries.Clear();
            if (values != null) foreach (var value in values) Record(value);
        }
    }

    [Serializable]
    public sealed class AquariumStatisticsState
    {
        public double ActiveSeconds { get; private set; }
        public int Activations { get; private set; }
        public void Activate() => Activations++;
        public void AddActiveTime(double seconds) { if (double.IsFinite(seconds) && seconds > 0) ActiveSeconds += seconds; }
        public void Restore(double activeSeconds, int activations)
        {
            ActiveSeconds = double.IsFinite(activeSeconds) ? Math.Max(0d, activeSeconds) : 0d;
            Activations = Math.Max(0, activations);
        }
    }

    [Serializable]
    public sealed class NitrogenCycleRuntimeState
    {
        public double SimplifiedSimulatedSeconds { get; private set; }
        public void Tick(double seconds) { if (double.IsFinite(seconds) && seconds > 0) SimplifiedSimulatedSeconds += seconds; }
        public void Restore(double seconds) =>
            SimplifiedSimulatedSeconds = double.IsFinite(seconds) ? Math.Max(0d, seconds) : 0d;
    }

    [Serializable]
    public sealed class AquariumInstance
    {
        public string InstanceId { get; }
        public string Name { get; private set; }
        public AquariumDefinition Definition { get; }
        public AquariumRuntimeState RuntimeState { get; }
        public AquariumHabitatProfile HabitatProfile { get; private set; }
        public AquariumFishCollection FishCollection { get; }
        public AquariumDecorationCollection DecorationCollection { get; }
        public WaterChemistryState WaterState { get; private set; }
        public NitrogenCycleRuntimeState NitrogenCycleState { get; }
        public AquariumMaintenanceState MaintenanceState { get; }
        public FilterRuntimeState FilterState { get; } = new();
        public AquariumJournalState JournalState { get; }
        public AquariumStatisticsState StatisticsState { get; }
        public string SlotId { get; internal set; }
        public string VisualRootId { get; internal set; }
        public bool IsInitialized { get; private set; }
        public bool IsActive { get; internal set; }
        public event Action<AquariumInstance> SummaryChanged;

        internal AquariumInstance(string id, string name, AquariumDefinition definition, AquariumRuntimeState runtime,
            AquariumHabitatProfile habitat, WaterChemistryState water, AquariumMaintenanceState maintenance)
        {
            InstanceId = id; Name = name; Definition = definition; RuntimeState = runtime;
            HabitatProfile = habitat ?? new AquariumHabitatProfile(); WaterState = water;
            MaintenanceState = maintenance; FishCollection = new AquariumFishCollection();
            DecorationCollection = new AquariumDecorationCollection(); NitrogenCycleState = new NitrogenCycleRuntimeState();
            JournalState = new AquariumJournalState(); StatisticsState = new AquariumStatisticsState();
            FishCollection.Changed += NotifySummaryChanged;
            IsInitialized = true;
        }

        public void SetName(string value) { if (!string.IsNullOrWhiteSpace(value)) { Name = value.Trim(); NotifySummaryChanged(); } }
        public void SetHabitat(AquariumHabitatProfile value) => HabitatProfile = value ?? new AquariumHabitatProfile();
        public void ReplaceWaterState(WaterChemistryState value) { if (value != null) WaterState = value; }
        public FilterRuntimeState EnsureFilterState(FilterDefinition definition)
        {
            if (!FilterState.IsInitialized && definition != null) FilterState.Initialize($"{InstanceId}-filter", definition);
            return FilterState;
        }
        public void TickInactive(double seconds) => NitrogenCycleState.Tick(seconds);
        public void SetTemperature(float value) { RuntimeState.SetTemperature(value); NotifySummaryChanged(); }
        public void AssignPresentation(string slotId,string visualRootId)
        {
            SlotId=slotId;
            VisualRootId=visualRootId;
        }
        public void NotifySummaryChanged() => SummaryChanged?.Invoke(this);
    }
}
