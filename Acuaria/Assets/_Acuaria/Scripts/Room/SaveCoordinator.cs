using System;
using System.Collections.Generic;
using Acuaria.Aquarium;
using Acuaria.Aquarium.Decorations;
using Acuaria.Aquarium.MultiAquarium;
using Acuaria.Fish;
using Acuaria.Save;
using Acuaria.Simulation.Water;
using UnityEngine;
using Acuaria.UI.Progression;
using Acuaria.Simulation.Filtration;

namespace Acuaria.Room
{
    public enum SaveStatus { Idle, Dirty, Saving, Saved, Loading, Loaded, Recovered, Failed, FutureVersion }

    [DefaultExecutionOrder(-1100)]
    public sealed class SaveCoordinator : MonoBehaviour
    {
        [SerializeField] AquariumManager manager;
        [SerializeField] AquariumDefinition[] aquariumDefinitions = Array.Empty<AquariumDefinition>();
        [SerializeField] DecorationRegistry decorationRegistry;
        [SerializeField] WaterChemistryDefinition waterDefinition;
        [SerializeField] FilterDefinition filterDefinition;
        [SerializeField] AquariumViewBinding[] bindings = Array.Empty<AquariumViewBinding>();
        [SerializeField] AquaristJournalController journal;
        [SerializeField, Min(.5f)] float debounceSeconds = 2f;
        [SerializeField, Min(1f)] float minimumSaveInterval = 5f;
        SaveMapper mapper; SaveService service; AcuariaSaveData current;
        float dirtyAt, lastSaveAt, nextFingerprintAt; int fingerprint; bool dirty, initialized;
        public SaveStatus Status { get; private set; } = SaveStatus.Idle;
        public string LastMessage { get; private set; }
        public string SavePath { get; private set; }
        public event Action<SaveStatus, string> StatusChanged;
        public void Configure(AquariumManager source, AquariumDefinition[] definitions, DecorationRegistry registry,
            WaterChemistryDefinition chemistry, AquariumViewBinding[] viewBindings, AquaristJournalController journalController = null,
            FilterDefinition filter = null)
        {
            manager = source; aquariumDefinitions = definitions ?? Array.Empty<AquariumDefinition>();
            decorationRegistry = registry; waterDefinition = chemistry; bindings = viewBindings ?? Array.Empty<AquariumViewBinding>();
            journal = journalController; filterDefinition = filter;
        }

        void Awake()
        {
            manager ??= AquariumManager.Instance;
            mapper = new SaveMapper(aquariumDefinitions, decorationRegistry, waterDefinition, filterDefinition);
            var storage = new SaveFileStorage(Application.persistentDataPath);
            SavePath = storage.MainPath;
            service = new SaveService(new JsonUtilitySaveSerializer(), storage, new SaveValidator(), new SaveMigrationPipeline());
            LoadOrCreate();
            initialized = true;
        }
        void OnEnable()
        {
            if (manager == null) return;
            manager.OnAquariumCreated += OnChanged; manager.OnAquariumRemoved += OnChanged;
            manager.OnAquariumActivated += OnChanged; SubscribeRuntime();
        }
        void Start()
        {
            SubscribeRuntime(); fingerprint = CalculateFingerprint();
            if (current != null && journal != null)
                mapper.ApplyProgress(current, journal.Player, journal.Missions, journal.Codex, journal.Achievements);
        }
        void OnDisable()
        {
            if (manager == null) return;
            manager.OnAquariumCreated -= OnChanged; manager.OnAquariumRemoved -= OnChanged; manager.OnAquariumActivated -= OnChanged;
            foreach (var aquarium in manager.Aquariums) aquarium.RuntimeState.Changed -= MarkDirty;
        }
        void Update()
        {
            if (!initialized) return;
            if (Time.unscaledTime >= nextFingerprintAt)
            {
                nextFingerprintAt = Time.unscaledTime + 1f;
                var next = CalculateFingerprint();
                if (next != fingerprint) { fingerprint = next; MarkDirty(); SubscribeRuntime(); }
            }
            if (dirty && Time.unscaledTime - dirtyAt >= debounceSeconds && Time.unscaledTime - lastSaveAt >= minimumSaveInterval) SaveNow();
        }
        void LoadOrCreate()
        {
            SetStatus(SaveStatus.Loading, "Loading local save.");
            var result = service.Load();
            if (result.Success && mapper.Apply(result.Data, manager, AreaForSlot))
            {
                current = result.Data;
                SetStatus(result.Source == SaveLoadSource.Backup ? SaveStatus.Recovered : SaveStatus.Loaded,
                    result.Source == SaveLoadSource.Backup ? "Recovered from backup." : "Loaded local save.");
                return;
            }
            if (result.Validation?.IsFutureVersion == true)
            {
                SetStatus(SaveStatus.FutureVersion, result.Message);
                return;
            }
            current = null;
            SetStatus(SaveStatus.Idle, "No valid save; scene will create a safe new game.");
        }
        public bool SaveNow()
        {
            if (Status == SaveStatus.FutureVersion || manager == null) return false;
            SetStatus(SaveStatus.Saving, "Saving.");
            var snapshot = mapper.Capture(manager, BuildSlots(), AreaForAquarium, current);
            if (journal != null) mapper.CaptureProgress(snapshot, journal.Player, journal.Missions, journal.Codex, journal.Achievements);
            if (snapshot.Aquariums.Count == 0) { SetStatus(SaveStatus.Failed, "No aquarium exists to save."); return false; }
            var result = service.Save(snapshot);
            if (!result.Success) { SetStatus(SaveStatus.Failed, result.Message); return false; }
            current = snapshot; dirty = false; lastSaveAt = Time.unscaledTime; fingerprint = CalculateFingerprint();
            SetStatus(SaveStatus.Saved, "Local save completed."); return true;
        }
        public void MarkDirty()
        {
            if (!initialized || Status == SaveStatus.FutureVersion) return;
            dirty = true; dirtyAt = Time.unscaledTime; SetStatus(SaveStatus.Dirty, "Unsaved changes.");
        }
        void OnChanged(AquariumInstance _) { SubscribeRuntime(); MarkDirty(); }
        void SubscribeRuntime()
        {
            if (manager == null) return;
            foreach (var aquarium in manager.Aquariums)
            {
                aquarium.RuntimeState.Changed -= MarkDirty;
                aquarium.RuntimeState.Changed += MarkDirty;
            }
        }
        List<AquariumSlotSaveData> BuildSlots()
        {
            var result = new List<AquariumSlotSaveData>();
            for (var index = 0; index < 3; index++)
            {
                var id = $"slot-{index + 1:00}"; AquariumInstance assigned = null;
                foreach (var aquarium in manager.Aquariums) if (aquarium.SlotId == id) { assigned = aquarium; break; }
                assigned ??= index < manager.Aquariums.Count ? manager.Aquariums[index] : null;
                result.Add(new AquariumSlotSaveData { SlotId = id, SlotIndex = index, SortOrder = index,
                    SlotState = assigned == null ? "Empty" : "Occupied", AssignedAquariumId = assigned?.InstanceId, IsVisible = true });
            }
            return result;
        }
        AquariumSwimArea2D AreaForSlot(string slotId)
        {
            foreach (var binding in bindings) if (binding != null && binding.SlotId == slotId) return binding.FishSpawner?.SwimArea;
            return null;
        }
        AquariumSwimArea2D AreaForAquarium(AquariumInstance aquarium) => AreaForSlot(aquarium?.SlotId);
        int CalculateFingerprint()
        {
            unchecked
            {
                var value = manager?.ActiveAquarium?.InstanceId?.GetHashCode() ?? 0;
                if (manager == null) return value;
                foreach (var aquarium in manager.Aquariums)
                {
                    value = value * 31 + aquarium.RuntimeState.LogicalTimestamp.GetHashCode();
                    value = value * 31 + aquarium.FishCollection.Count;
                    value = value * 31 + aquarium.DecorationCollection.Placements.Count;
                    value = value * 31 + (aquarium.WaterState?.Version.GetHashCode() ?? 0);
                    value = value * 31 + aquarium.MaintenanceState.ChangesPerformed;
                    value = value * 31 + aquarium.JournalState.Entries.Count;
                }
                return value;
            }
        }
        void OnApplicationPause(bool paused) { if (paused) SaveNow(); }
        void OnApplicationFocus(bool focused) { if (!focused) SaveNow(); }
        void OnApplicationQuit()
        {
            if (current != null) current.LastSessionEndedAtUtc = DateTime.UtcNow.ToString("O");
            SaveNow();
        }
        void SetStatus(SaveStatus value, string message) { Status = value; LastMessage = message; StatusChanged?.Invoke(value, message); }
    }
}
