using System;
using System.Collections.Generic;
using Acuaria.Aquarium;
using Acuaria.Aquarium.Decorations;
using Acuaria.Aquarium.MultiAquarium;
using Acuaria.Fish;
using Acuaria.Fish.Care;
using Acuaria.Simulation.Maintenance;
using Acuaria.Simulation.Water;
using Acuaria.Progression;
using Acuaria.Simulation.Filtration;
using UnityEngine;

namespace Acuaria.Save
{
    public sealed class SaveMapper
    {
        readonly IReadOnlyList<AquariumDefinition> definitions;
        readonly DecorationRegistry decorations;
        readonly WaterChemistryDefinition chemistry;
        readonly FilterDefinition filterDefinition;
        public SaveMapper(IReadOnlyList<AquariumDefinition> aquariumDefinitions, DecorationRegistry decorationRegistry,
            WaterChemistryDefinition waterDefinition, FilterDefinition filter = null)
        { definitions = aquariumDefinitions; decorations = decorationRegistry; chemistry = waterDefinition; filterDefinition = filter; }

        public AcuariaSaveData Capture(AquariumManager manager, IReadOnlyList<AquariumSlotSaveData> slots,
            Func<AquariumInstance, AquariumSwimArea2D> swimAreaResolver = null, AcuariaSaveData previous = null)
        {
            var now = DateTime.UtcNow.ToString("O");
            var data = new AcuariaSaveData
            {
                GameVersion = Application.version,
                SaveId = previous?.SaveId ?? Guid.NewGuid().ToString("N"),
                CreatedAtUtc = previous?.CreatedAtUtc ?? now,
                UpdatedAtUtc = now,
                LastSessionStartedAtUtc = previous?.LastSessionStartedAtUtc ?? now,
                ActiveAquariumId = manager?.ActiveAquarium?.InstanceId,
                AquariumSlots = slots != null ? new List<AquariumSlotSaveData>(slots) : new List<AquariumSlotSaveData>()
            };
            if (manager == null) return data;
            foreach (var aquarium in manager.Aquariums)
            {
                AquariumSaveData previousAquarium = null;
                if (previous?.Aquariums != null) previousAquarium = previous.Aquariums.Find(item => item.AquariumInstanceId == aquarium.InstanceId);
                data.Aquariums.Add(CaptureAquarium(aquarium, swimAreaResolver?.Invoke(aquarium), now, previousAquarium?.CreatedAtUtc));
            }
            data.PlayerProgress.CreatedAquariumCount = data.Aquariums.Count;
            data.PlayerProgress.LastSelectedAquariumId = data.ActiveAquariumId;
            return data;
        }

        AquariumSaveData CaptureAquarium(AquariumInstance aquarium, AquariumSwimArea2D area, string now, string createdAt)
        {
            var runtime = aquarium.RuntimeState; var water = aquarium.WaterState; var maintenance = aquarium.MaintenanceState;
            var result = new AquariumSaveData
            {
                AquariumInstanceId = aquarium.InstanceId, DisplayName = aquarium.Name,
                AquariumDefinitionId = aquarium.Definition.AquariumId, SlotId = aquarium.SlotId,
                CreatedAtUtc = string.IsNullOrWhiteSpace(createdAt) ? now : createdAt, UpdatedAtUtc = now, IsInitialized = aquarium.IsInitialized,
                RuntimeState = new AquariumRuntimeSaveData
                {
                    VolumeLiters = aquarium.Definition.NominalVolumeLitres, TemperatureCelsius = runtime.CurrentTemperature,
                    CurrentFishCount = aquarium.FishCollection.Count, IsAvailable = runtime.IsAvailable,
                    IsFocused = runtime.IsFocused, LogicalTimestamp = runtime.LogicalTimestamp
                },
                WaterState = new WaterStateSaveData
                {
                    TemperatureCelsius = runtime.CurrentTemperature, AmmoniaPpm = water?.AmmoniaMgPerLiter ?? 0,
                    NitritePpm = water?.NitriteMgPerLiter ?? 0, NitratePpm = water?.NitrateMgPerLiter ?? 0,
                    AmmoniaBacteria = water?.AmmoniaOxidizingBacteria ?? 0, NitriteBacteria = water?.NitriteOxidizingBacteria ?? 0,
                    OrganicWaste = water?.OrganicWaste ?? 0, TotalSimulatedSeconds = water?.TotalSimulatedSeconds ?? 0,
                    LastSimulationStep = water?.LastSimulationStep ?? 0, AmmoniaTrend = (int)(water?.AmmoniaTrend ?? 0),
                    NitriteTrend = (int)(water?.NitriteTrend ?? 0), NitrateTrend = (int)(water?.NitrateTrend ?? 0), LastUpdatedAtUtc = now
                },
                NitrogenCycleState = new NitrogenCycleSaveData { TotalElapsedSimulationSeconds = aquarium.NitrogenCycleState.SimplifiedSimulatedSeconds, LastTickAtUtc = now },
                MaintenanceState = new MaintenanceSaveData
                {
                    CurrentStableState = AquariumMaintenancePhase.Idle.ToString(), LastResult = (int)maintenance.LastResult,
                    LastPercentage = maintenance.LastPercentage, CooldownRemaining = maintenance.CooldownRemaining,
                    TotalWaterChangedPercent = maintenance.TotalWaterChangedPercent, WaterChangeCount = maintenance.ChangesPerformed
                },
                FilterState = new FilterStateSaveData
                {
                    FilterDefinitionId = aquarium.FilterState.DefinitionId,
                    BiologicalCapacity = aquarium.FilterState.BiologicalCapacity, Efficiency = aquarium.FilterState.CurrentEfficiency,
                    DirtLevel = aquarium.FilterState.DirtLevel, HoursSinceMaintenance = aquarium.FilterState.HoursSinceMaintenance,
                    IsRunning = aquarium.FilterState.IsActive, MaintenanceRecommended = aquarium.FilterState.MaintenanceRecommended,
                    Status = (int)aquarium.FilterState.Status, MaintenanceCount = aquarium.FilterState.MaintenanceCount
                },
                HabitatState = new HabitatSaveData
                {
                    PlantCoverage = aquarium.HabitatProfile.PlantCoverageAmount, ShelterScore = aquarium.HabitatProfile.HidingPlaceCount,
                    OpenSwimmingSpace = aquarium.HabitatProfile.OpenSwimmingSpace, Complexity = aquarium.HabitatProfile.VisualComplexity,
                    LastEvaluatedAtUtc = now
                },
                Journal = new AquariumJournalSaveData { AquariumInstanceId = aquarium.InstanceId },
                Statistics = new StatisticsSaveData { TotalPlayTimeSeconds = aquarium.StatisticsState.ActiveSeconds, AquariumVisits = aquarium.StatisticsState.Activations }
            };
            var journalIndex = 0;
            foreach (var entry in aquarium.JournalState.Entries) result.Journal.Entries.Add(new JournalEntrySaveData
            { EntryId = $"{aquarium.InstanceId}-journal-{++journalIndex}", AquariumInstanceId = aquarium.InstanceId, Content = entry, TimestampUtc = now, IsRead = true });
            foreach (var fish in aquarium.FishCollection.Fish)
            {
                var normalized = Normalize(fish.Position, area);
                result.Fish.Add(new FishSaveData
                {
                    FishInstanceId = fish.InstanceId, AquariumInstanceId = aquarium.InstanceId, SpeciesDefinitionId = fish.SpeciesId,
                    NormalizedPosition = new SerializableVector2(normalized.x, normalized.y),
                    FacingDirection = new SerializableVector2(fish.Direction.x, fish.Direction.y), RandomSeed = fish.RandomSeed,
                    Satiety = Mathf.Clamp01(fish.Satiety), CreatedAtUtc = now, LastUpdatedAtUtc = now
                });
            }
            foreach (var placement in aquarium.DecorationCollection.Placements)
                if (placement?.IsValid == true) result.Decorations.Add(new DecorationSaveData
                {
                    DecorationInstanceId = placement.InstanceId, AquariumInstanceId = aquarium.InstanceId,
                    DecorationDefinitionId = placement.Definition.DecorationId,
                    NormalizedPosition = new SerializableVector2(placement.NormalizedPosition.x, placement.NormalizedPosition.y),
                    Scale = new SerializableVector2(placement.LocalScale.x, placement.LocalScale.y),
                    RotationDegrees = placement.LocalRotation, FlipX = placement.FlipX,
                    SortingOrder = placement.SortingOrderOffset, VisualLayer = (int)placement.VisualLayer,
                    IsVisible = placement.IsVisible, CreatedAtUtc = now, LastModifiedAtUtc = now
                });
            return result;
        }

        public bool Apply(AcuariaSaveData data, AquariumManager manager, Func<string, AquariumSwimArea2D> areaResolver = null)
        {
            if (data == null || manager == null) return false;
            manager.ResetForLoad();
            foreach (var saved in data.Aquariums)
            {
                var definition = FindDefinition(saved.AquariumDefinitionId);
                if (definition == null) { Debug.LogWarning($"Save skipped aquarium with missing definition '{saved.AquariumDefinitionId}'."); continue; }
                var aquarium = manager.CreateAquarium(definition, saved.AquariumInstanceId, saved.DisplayName);
                aquarium.AssignPresentation(saved.SlotId, null);
                aquarium.SetTemperature(saved.RuntimeState.TemperatureCelsius);
                aquarium.RuntimeState.SetAvailable(saved.RuntimeState.IsAvailable);
                RestoreWater(aquarium, saved);
                aquarium.NitrogenCycleState.Restore(saved.NitrogenCycleState?.TotalElapsedSimulationSeconds ?? 0);
                var maintenance = saved.MaintenanceState;
                if (maintenance != null) aquarium.MaintenanceState.RestoreStable($"{aquarium.InstanceId}-maintenance",
                    (AquariumMaintenanceResult)maintenance.LastResult, maintenance.LastPercentage, maintenance.CooldownRemaining,
                    maintenance.TotalWaterChangedPercent, maintenance.WaterChangeCount);
                var filter = saved.FilterState;
                if (filterDefinition != null && filter != null) aquarium.FilterState.Restore($"{aquarium.InstanceId}-filter",
                    filterDefinition, filter.IsRunning, filter.DirtLevel, filter.Efficiency, filter.BiologicalCapacity,
                    filter.HoursSinceMaintenance, (FilterOperatingStatus)filter.Status, filter.MaintenanceRecommended, filter.MaintenanceCount);
                var fish = new List<FishRuntimeState>();
                var area = areaResolver?.Invoke(saved.SlotId);
                foreach (var value in saved.Fish ?? new List<FishSaveData>())
                {
                    if (string.IsNullOrWhiteSpace(value?.FishInstanceId) || string.IsNullOrWhiteSpace(value.SpeciesDefinitionId)) continue;
                    var position = Denormalize(value.NormalizedPosition, area);
                    var state = new FishRuntimeState(); state.Initialize(value.FishInstanceId, value.SpeciesDefinitionId, position, value.RandomSeed);
                    state.Direction = SafeDirection(value.FacingDirection); state.Satiety = Mathf.Clamp01(value.Satiety); fish.Add(state);
                }
                aquarium.FishCollection.Replace(fish); aquarium.RuntimeState.SetFishCount(fish.Count);
                var placements = new List<DecorationPlacementData>();
                foreach (var value in saved.Decorations ?? new List<DecorationSaveData>())
                {
                    var definitionValue = decorations?.FindById(value.DecorationDefinitionId);
                    if (definitionValue == null) { Debug.LogWarning($"Save skipped missing decoration '{value.DecorationDefinitionId}'."); continue; }
                    placements.Add(new DecorationPlacementData(value.DecorationInstanceId, definitionValue,
                        new Vector2(value.NormalizedPosition.X, value.NormalizedPosition.Y), new Vector2(value.Scale.X, value.Scale.Y),
                        value.RotationDegrees, value.FlipX, value.SortingOrder, (DecorationVisualLayer)value.VisualLayer));
                }
                aquarium.DecorationCollection.Replace(placements);
                var installed = new List<DecorationDefinition>(); foreach (var placement in placements) installed.Add(placement.Definition);
                aquarium.SetHabitat(AquariumHabitatCalculator.Calculate(installed));
                var journalEntries = new List<string>();
                foreach (var entry in saved.Journal?.Entries ?? new List<JournalEntrySaveData>()) if (!string.IsNullOrWhiteSpace(entry.Content)) journalEntries.Add(entry.Content);
                aquarium.JournalState.Restore(journalEntries);
                aquarium.StatisticsState.Restore(saved.Statistics?.TotalPlayTimeSeconds ?? 0, saved.Statistics?.AquariumVisits ?? 0);
            }
            if (manager.Aquariums.Count == 0) return false;
            if (!manager.RestoreActiveAquarium(data.ActiveAquariumId) && manager.ActiveAquarium == null)
                manager.RestoreActiveAquarium(manager.Aquariums[0].InstanceId);
            return true;
        }

        public void CaptureProgress(AcuariaSaveData data, PlayerProgression player, MissionController missions,
            CodexController codex, AchievementController achievements)
        {
            if (data == null || player == null) return;
            var statistics = player.Statistics;
            data.PlayerProgress ??= new PlayerProgressSaveData();
            data.PlayerProgress.Experience = player.Experience.TotalXp;
            data.PlayerProgress.Level = player.Experience.Level.Number;
            data.PlayerProgress.GeneralStatistics = new StatisticsSaveData
            {
                FeedActions = statistics.MealsGiven, WaterChanges = statistics.WaterChanges,
                FilterCleanings = statistics.FilterCleanings, SimulatedHours = statistics.SimulatedHours,
                XpEarned = statistics.XpEarned, ExcellentWaterHours = statistics.ExcellentWaterHours,
                ExcellentWelfareHours = statistics.ExcellentWelfareHours, WastedFood = statistics.WastedFood
            };
            data.PlayerProgress.MissionProgress.Clear();
            if (missions != null) for (var i = 0; i < missions.States.Count; i++)
            {
                var state = missions.States[i]; var definition = missions.Definitions[i];
                data.PlayerProgress.MissionProgress.Add(new MissionProgressSaveData
                {
                    MissionDefinitionId = state.MissionId, Status = state.Status.ToString(),
                    CurrentProgress = state.Progress, TargetProgress = definition.Target,
                    RewardClaimed = state.Status == MissionStatus.Claimed
                });
            }
            data.PlayerProgress.UnlockedJournalEntries.Clear();
            if (codex != null) for (var i = 0; i < codex.States.Count; i++)
                if (codex.States[i].IsUnlocked) data.PlayerProgress.UnlockedJournalEntries.Add(codex.Entries[i].EntryId);
            data.PlayerProgress.AchievementProgress.Clear();
            if (achievements != null) for (var i = 0; i < achievements.States.Count; i++)
                data.PlayerProgress.AchievementProgress.Add(new AchievementProgressSaveData
                {
                    AchievementDefinitionId = achievements.States[i].AchievementId,
                    Progress = achievements.States[i].Progress, IsUnlocked = achievements.States[i].IsUnlocked
                });
        }

        public void ApplyProgress(AcuariaSaveData data, PlayerProgression player, MissionController missions,
            CodexController codex, AchievementController achievements)
        {
            var saved = data?.PlayerProgress; if (saved == null || player == null) return;
            player.Experience.Restore(saved.Experience);
            var statistics = saved.GeneralStatistics ?? new StatisticsSaveData();
            player.Statistics.Restore(statistics.FeedActions, statistics.WaterChanges, statistics.FilterCleanings,
                statistics.SimulatedHours, statistics.XpEarned, statistics.ExcellentWaterHours,
                statistics.ExcellentWelfareHours, statistics.WastedFood);
            if (missions != null) foreach (var value in saved.MissionProgress ?? new List<MissionProgressSaveData>())
                for (var i = 0; i < missions.States.Count; i++)
                    if (missions.States[i].MissionId == value.MissionDefinitionId &&
                        Enum.TryParse<MissionStatus>(value.Status, out var status))
                        missions.States[i].Restore(value.CurrentProgress, status, missions.Definitions[i].Target);
            if (codex != null) for (var i = 0; i < codex.Entries.Count; i++)
                codex.States[i].Restore(codex.Entries[i].EntryId,
                    saved.UnlockedJournalEntries?.Contains(codex.Entries[i].EntryId) == true);
            if (achievements != null) foreach (var value in saved.AchievementProgress ?? new List<AchievementProgressSaveData>())
                for (var i = 0; i < achievements.States.Count; i++)
                    if (achievements.States[i].AchievementId == value.AchievementDefinitionId)
                        achievements.States[i].Restore(value.Progress, value.IsUnlocked, achievements.Definitions[i].Target);
            player.NotifyChanged();
        }

        void RestoreWater(AquariumInstance aquarium, AquariumSaveData saved)
        {
            if (chemistry == null || saved.WaterState == null) return;
            var value = saved.WaterState; var state = new WaterChemistryState();
            state.Restore($"{aquarium.InstanceId}-water", value.AmmoniaPpm, value.NitritePpm, value.NitratePpm,
                value.AmmoniaBacteria, value.NitriteBacteria, value.OrganicWaste, value.TotalSimulatedSeconds,
                value.LastSimulationStep, (WaterParameterTrend)value.AmmoniaTrend, (WaterParameterTrend)value.NitriteTrend,
                (WaterParameterTrend)value.NitrateTrend, chemistry);
            aquarium.ReplaceWaterState(state);
        }
        AquariumDefinition FindDefinition(string id)
        {
            if (definitions != null) foreach (var value in definitions)
                if (value != null && string.Equals(value.AquariumId, id, StringComparison.Ordinal)) return value;
            return null;
        }
        static Vector2 Normalize(Vector2 local, AquariumSwimArea2D area)
        {
            if (area == null) return new Vector2(Mathf.Clamp01(local.x), Mathf.Clamp01(local.y));
            var bounds = area.NavigationBounds;
            return new Vector2(Mathf.InverseLerp(bounds.Left, bounds.Right, local.x), Mathf.InverseLerp(bounds.Bottom, bounds.Top, local.y));
        }
        static Vector2 Denormalize(SerializableVector2 value, AquariumSwimArea2D area)
        {
            var normalized = new Vector2(Mathf.Clamp01(value.X), Mathf.Clamp01(value.Y));
            if (area == null) return normalized;
            var bounds = area.NavigationBounds;
            return area.ClampLocal(new Vector2(Mathf.Lerp(bounds.Left, bounds.Right, normalized.x), Mathf.Lerp(bounds.Bottom, bounds.Top, normalized.y)));
        }
        static Vector2 SafeDirection(SerializableVector2 value)
        {
            var direction = new Vector2(float.IsFinite(value.X) ? value.X : 1f, float.IsFinite(value.Y) ? value.Y : 0f);
            return direction.sqrMagnitude < .0001f ? Vector2.right : direction.normalized;
        }
    }
}
