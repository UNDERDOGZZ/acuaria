using System;
using System.Collections.Generic;

namespace Acuaria.Save
{
    public static class SaveSystemDefinition
    {
        public const string FormatId = "ACUARIA_LOCAL_SAVE";
        public const int CurrentSchemaVersion = 2;
        public const string MainFileName = "acuaria_save.json";
        public const string BackupFileName = "acuaria_save.backup.json";
        public const string TemporaryFileName = "acuaria_save.tmp.json";
    }

    [Serializable] public struct SerializableVector2
    {
        public float X, Y;
        public SerializableVector2(float x, float y) { X = x; Y = y; }
    }

    [Serializable] public sealed class AcuariaSaveData
    {
        public string SaveFormatId = SaveSystemDefinition.FormatId;
        public int SchemaVersion = SaveSystemDefinition.CurrentSchemaVersion;
        public string GameVersion, SaveId, CreatedAtUtc, UpdatedAtUtc, LastSessionStartedAtUtc, LastSessionEndedAtUtc;
        public string LastSimulationAtUtc, LastApplicationPauseAtUtc, LastApplicationResumeAtUtc;
        public string LastAppliedOfflineIntervalStartUtc, LastAppliedOfflineIntervalEndUtc, LastOfflineExecutionKey;
        public int OfflineSimulationSequence;
        public string SimulationVersion = "1";
        public string ActiveAquariumId;
        public PlayerProgressSaveData PlayerProgress = new();
        public List<AquariumSlotSaveData> AquariumSlots = new();
        public List<AquariumSaveData> Aquariums = new();
        public AquariumJournalSaveData GlobalJournal = new();
        public StatisticsSaveData GlobalStatistics = new();
        public IntegritySaveData IntegrityData = new();
        public List<string> MigrationHistory = new();
    }

    [Serializable] public sealed class IntegritySaveData { public string Algorithm = "SHA256"; public string Checksum; }
    [Serializable] public sealed class PlayerProgressSaveData
    {
        public int Experience, Level;
        public List<string> CompletedTutorials = new(), UnlockedJournalEntries = new(), SpeciesDiscovered = new(), DecorationsDiscovered = new();
        public List<MissionProgressSaveData> MissionProgress = new();
        public List<AchievementProgressSaveData> AchievementProgress = new();
        public StatisticsSaveData GeneralStatistics = new();
        public int CreatedAquariumCount;
        public string LastSelectedAquariumId;
    }
    [Serializable] public sealed class AquariumSlotSaveData
    {
        public string SlotId, SlotState, AssignedAquariumId;
        public int SlotIndex, SortOrder;
        public bool IsVisible = true, IsDevelopmentUnlocked = true;
    }
    [Serializable] public sealed class AquariumSaveData
    {
        public string AquariumInstanceId, DisplayName, AquariumDefinitionId, SlotId, CreatedAtUtc, UpdatedAtUtc;
        public AquariumRuntimeSaveData RuntimeState = new();
        public WaterStateSaveData WaterState = new();
        public NitrogenCycleSaveData NitrogenCycleState = new();
        public FilterStateSaveData FilterState = new();
        public MaintenanceSaveData MaintenanceState = new();
        public HabitatSaveData HabitatState = new();
        public WelfareSaveData WelfareState = new();
        public List<FishSaveData> Fish = new();
        public List<DecorationSaveData> Decorations = new();
        public AquariumJournalSaveData Journal = new();
        public StatisticsSaveData Statistics = new();
        public bool IsInitialized;
    }
    [Serializable] public sealed class AquariumRuntimeSaveData
    {
        public float VolumeLiters, TemperatureCelsius;
        public int CurrentFishCount;
        public bool IsAvailable, IsFocused;
        public long LogicalTimestamp;
    }
    [Serializable] public sealed class WaterStateSaveData
    {
        public float TemperatureCelsius, PH, GH, KH, AmmoniaPpm, NitritePpm, NitratePpm;
        public float AmmoniaBacteria, NitriteBacteria, OrganicWaste, LastSimulationStep;
        public double TotalSimulatedSeconds;
        public int AmmoniaTrend, NitriteTrend, NitrateTrend;
        public string LastUpdatedAtUtc;
    }
    [Serializable] public sealed class NitrogenCycleSaveData
    {
        public string Stage;
        public float Progress, AmmoniaProcessingCapacity, NitriteProcessingCapacity, BacteriaLevel;
        public bool IsEstablished;
        public string LastTickAtUtc;
        public double TotalElapsedSimulationSeconds;
    }
    [Serializable] public sealed class FilterStateSaveData
    {
        public string FilterDefinitionId;
        public float BiologicalCapacity, Efficiency, DirtLevel, HoursSinceMaintenance;
        public bool IsRunning, MaintenanceRecommended;
        public int Status, MaintenanceCount;
    }
    [Serializable] public sealed class MaintenanceSaveData
    {
        public string LastWaterChangeAtUtc, LastFilterCleaningAtUtc, CurrentStableState;
        public int WaterChangeCount, FilterCleaningCount, LastPercentage, LastResult;
        public float CooldownRemaining, TotalWaterChangedPercent;
        public List<string> MaintenanceHistory = new();
    }
    [Serializable] public sealed class FishSaveData
    {
        public string FishInstanceId, AquariumInstanceId, SpeciesDefinitionId, DisplayName, CreatedAtUtc, LastFedAtUtc, LastUpdatedAtUtc;
        public float Age, Size, GrowthProgress, Hunger, Satiety, Health = 1f, Stress, Welfare = 1f;
        public SerializableVector2 NormalizedPosition, FacingDirection;
        public int RandomSeed;
        public bool IsAlive = true;
    }
    [Serializable] public sealed class DecorationSaveData
    {
        public string DecorationInstanceId, AquariumInstanceId, DecorationDefinitionId, CreatedAtUtc, LastModifiedAtUtc;
        public SerializableVector2 NormalizedPosition, Scale;
        public float RotationDegrees;
        public bool FlipX, IsVisible = true, IsMovable = true, IsRemovable = true;
        public int SortingOrder, VisualLayer;
    }
    [Serializable] public sealed class HabitatSaveData
    {
        public float PlantCoverage, ShelterScore, OpenSwimmingSpace, Complexity, HabitatBalance;
        public List<string> HabitatWarnings = new();
        public string LastEvaluatedAtUtc;
    }
    [Serializable] public sealed class WelfareSaveData
    {
        public float OverallScore, WaterScore, HabitatScore, SocialScore, NutritionScore, StressScore;
        public List<string> ActiveWarnings = new();
        public string LastEvaluatedAtUtc;
    }
    [Serializable] public sealed class AquariumJournalSaveData
    {
        public string AquariumInstanceId;
        public List<JournalEntrySaveData> Entries = new();
        public List<string> DiscoveredConcepts = new(), EducationalFlags = new();
    }
    [Serializable] public sealed class JournalEntrySaveData
    {
        public string EntryId, EntryType, DefinitionId, TimestampUtc, AquariumInstanceId, RelatedFishId, RelatedDecorationId, RelatedMissionId, Content;
        public bool IsRead, IsImportant;
    }
    [Serializable] public sealed class MissionProgressSaveData
    {
        public string MissionDefinitionId, Status, StartedAtUtc, CompletedAtUtc, AquariumInstanceId;
        public int CurrentProgress, TargetProgress;
        public bool RewardClaimed;
    }
    [Serializable] public sealed class AchievementProgressSaveData
    {
        public string AchievementDefinitionId, UnlockedAtUtc;
        public bool IsUnlocked, RewardClaimed;
        public int Progress;
    }
    [Serializable] public sealed class StatisticsSaveData
    {
        public double TotalPlayTimeSeconds, SimulatedHours, ExcellentWaterHours, ExcellentWelfareHours;
        public int AquariumVisits, AquariumSwitches, FeedActions, MaintenanceActions, WaterChanges, FilterCleanings,
            DecorationsAdded, DecorationsMoved, FishAdded, MissionsCompleted, JournalEntriesUnlocked, SaveCount, LoadCount, RecoveryCount, XpEarned, WastedFood;
        public int OfflineSessions, CappedOfflineSessions;
        public double TotalOfflineTimeSeconds;
    }
}
