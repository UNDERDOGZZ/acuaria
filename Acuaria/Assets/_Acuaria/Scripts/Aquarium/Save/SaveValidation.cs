using System;
using System.Collections.Generic;

namespace Acuaria.Save
{
    public enum SaveIssueSeverity { Warning, RecoverableError, CriticalError, FutureVersion }
    public sealed class SaveIssue
    {
        public SaveIssueSeverity Severity { get; }
        public string Code { get; }
        public string Message { get; }
        public SaveIssue(SaveIssueSeverity severity, string code, string message) { Severity = severity; Code = code; Message = message; }
    }
    public sealed class SaveValidationResult
    {
        readonly List<SaveIssue> issues = new();
        public IReadOnlyList<SaveIssue> Issues => issues;
        public bool IsValid { get; internal set; } = true;
        public bool IsFutureVersion { get; internal set; }
        internal void Add(SaveIssueSeverity severity, string code, string message)
        {
            issues.Add(new SaveIssue(severity, code, message));
            if (severity is SaveIssueSeverity.CriticalError or SaveIssueSeverity.FutureVersion) IsValid = false;
            if (severity == SaveIssueSeverity.FutureVersion) IsFutureVersion = true;
        }
    }
    public sealed class SaveValidator
    {
        public SaveValidationResult Validate(AcuariaSaveData data)
        {
            var result = new SaveValidationResult();
            if (data == null) { result.Add(SaveIssueSeverity.CriticalError, "null", "Save root is null."); return result; }
            if (!string.Equals(data.SaveFormatId, SaveSystemDefinition.FormatId, StringComparison.Ordinal))
                result.Add(SaveIssueSeverity.CriticalError, "format", "SaveFormatId does not belong to Acuaria.");
            if (data.SchemaVersion > SaveSystemDefinition.CurrentSchemaVersion)
                result.Add(SaveIssueSeverity.FutureVersion, "future-version", "Save was created by a newer schema.");
            else if (data.SchemaVersion <= 0) result.Add(SaveIssueSeverity.CriticalError, "schema", "SchemaVersion is invalid.");
            if (string.IsNullOrWhiteSpace(data.SaveId)) result.Add(SaveIssueSeverity.CriticalError, "save-id", "SaveId is required.");
            if (!Utc(data.CreatedAtUtc) || !Utc(data.UpdatedAtUtc))
                result.Add(SaveIssueSeverity.CriticalError, "timestamp", "CreatedAtUtc and UpdatedAtUtc must be valid UTC timestamps.");
            data.Aquariums ??= new List<AquariumSaveData>(); data.AquariumSlots ??= new List<AquariumSlotSaveData>();
            var aquariumIds = new HashSet<string>(StringComparer.Ordinal);
            var fishIds = new HashSet<string>(StringComparer.Ordinal);
            var decorationIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var aquarium in data.Aquariums)
            {
                if (aquarium == null || string.IsNullOrWhiteSpace(aquarium.AquariumInstanceId) || string.IsNullOrWhiteSpace(aquarium.AquariumDefinitionId))
                { result.Add(SaveIssueSeverity.CriticalError, "aquarium", "Aquarium identity is incomplete."); continue; }
                if (!aquariumIds.Add(aquarium.AquariumInstanceId)) result.Add(SaveIssueSeverity.CriticalError, "aquarium-duplicate", aquarium.AquariumInstanceId);
                if (!Finite(aquarium.RuntimeState?.TemperatureCelsius ?? float.NaN))
                    result.Add(SaveIssueSeverity.CriticalError, "temperature", aquarium.AquariumInstanceId);
                aquarium.Fish ??= new List<FishSaveData>();
                foreach (var fish in aquarium.Fish)
                {
                    if (fish == null || string.IsNullOrWhiteSpace(fish.FishInstanceId) || string.IsNullOrWhiteSpace(fish.SpeciesDefinitionId))
                    { result.Add(SaveIssueSeverity.RecoverableError, "fish", "Invalid fish skipped."); continue; }
                    if (!fishIds.Add(fish.FishInstanceId)) result.Add(SaveIssueSeverity.CriticalError, "fish-duplicate", fish.FishInstanceId);
                    if (!Finite(fish.NormalizedPosition.X) || !Finite(fish.NormalizedPosition.Y) || !Finite(fish.Satiety))
                        result.Add(SaveIssueSeverity.RecoverableError, "fish-numeric", fish.FishInstanceId);
                }
                aquarium.Decorations ??= new List<DecorationSaveData>();
                foreach (var decoration in aquarium.Decorations)
                {
                    if (decoration == null || string.IsNullOrWhiteSpace(decoration.DecorationInstanceId) || string.IsNullOrWhiteSpace(decoration.DecorationDefinitionId))
                    { result.Add(SaveIssueSeverity.RecoverableError, "decoration", "Invalid decoration skipped."); continue; }
                    if (!decorationIds.Add(decoration.DecorationInstanceId)) result.Add(SaveIssueSeverity.CriticalError, "decoration-duplicate", decoration.DecorationInstanceId);
                }
            }
            if (!string.IsNullOrWhiteSpace(data.ActiveAquariumId) && !aquariumIds.Contains(data.ActiveAquariumId))
                result.Add(SaveIssueSeverity.CriticalError, "active-orphan", data.ActiveAquariumId);
            var slotIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var slot in data.AquariumSlots)
            {
                if (slot == null || string.IsNullOrWhiteSpace(slot.SlotId) || !slotIds.Add(slot.SlotId))
                    result.Add(SaveIssueSeverity.CriticalError, "slot", "Slot identity is missing or duplicated.");
                else if (slot.SlotState == "Occupied" && !aquariumIds.Contains(slot.AssignedAquariumId))
                    result.Add(SaveIssueSeverity.CriticalError, "slot-orphan", slot.SlotId);
            }
            return result;
        }
        static bool Utc(string value) => DateTime.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind, out var date) && date.Kind == DateTimeKind.Utc;
        static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
    public interface ISaveMigration { int FromVersion { get; } int ToVersion { get; } AcuariaSaveData Migrate(AcuariaSaveData data); }
    public sealed class SaveMigrationPipeline
    {
        readonly List<ISaveMigration> migrations = new();
        public void Register(ISaveMigration migration) { if (migration != null) migrations.Add(migration); }
        public AcuariaSaveData Migrate(AcuariaSaveData data)
        {
            if (data == null || data.SchemaVersion > SaveSystemDefinition.CurrentSchemaVersion) return data;
            while (data.SchemaVersion < SaveSystemDefinition.CurrentSchemaVersion)
            {
                var migration = migrations.Find(item => item.FromVersion == data.SchemaVersion);
                if (migration == null) throw new InvalidOperationException($"No migration from schema {data.SchemaVersion}.");
                data = migration.Migrate(data); data.SchemaVersion = migration.ToVersion;
                data.MigrationHistory ??= new List<string>(); data.MigrationHistory.Add($"{migration.FromVersion}->{migration.ToVersion}");
            }
            return data;
        }
    }
}
