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
using Acuaria.Offline;

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
        [SerializeField] OfflineSimulationDefinition offlineDefinition;
        [SerializeField] AquariumViewBinding[] bindings = Array.Empty<AquariumViewBinding>();
        [SerializeField] AquaristJournalController journal;
        [SerializeField, Min(.5f)] float debounceSeconds = 2f;
        [SerializeField, Min(1f)] float minimumSaveInterval = 5f;
        SaveMapper mapper; SaveService service; AcuariaSaveData current;
        OfflineSimulationService offlineService; bool applicationPaused, applicationFocused=true, backgroundIntervalOpen;
        float dirtyAt, lastSaveAt, nextFingerprintAt; int fingerprint; bool dirty, initialized;
        public SaveStatus Status { get; private set; } = SaveStatus.Idle;
        public string LastMessage { get; private set; }
        public string SavePath { get; private set; }
        public event Action<SaveStatus, string> StatusChanged;
        public event Action<OfflineSimulationReport> OfflineProgressApplied;
        public OfflineSimulationReport LastOfflineReport{get;private set;}
        public void SetOfflineDefinition(OfflineSimulationDefinition value)=>offlineDefinition=value;
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
            offlineService=new OfflineSimulationService(new SystemOfflineTimeProvider());
            var storage = new SaveFileStorage(Application.persistentDataPath);
            SavePath = storage.MainPath;
            var migrations=new SaveMigrationPipeline();migrations.Register(new SaveMigrationV1ToV2());
            service = new SaveService(new JsonUtilitySaveSerializer(), storage, new SaveValidator(), migrations);
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
            {
                mapper.ApplyProgress(current, journal.Player, journal.Missions, journal.Codex, journal.Achievements);
                ApplyOfflineProgression(LastOfflineReport);
                if(LastOfflineReport?.Applied==true)PersistOfflineState();
            }
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
            if (result.Success)
            {
                current = result.Data;
                var offline=offlineDefinition!=null&&!offlineDefinition.AllowColdStart
                    ?new OfflineSimulationResult{Success=true,Data=current,Report=new OfflineSimulationReport()}
                    :offlineService.Simulate(current,OfflineSimulationPolicy.From(offlineDefinition),true);
                if(!offline.Success){SetStatus(SaveStatus.Failed,offline.Error??offline.Report?.Time?.Status.ToString());return;}
                current=offline.Data;
                if(!mapper.Apply(current, manager, AreaForSlot)){SetStatus(SaveStatus.Failed,"Runtime restoration failed.");return;}
                if(offline.Report?.Applied==true)
                {
                    LastOfflineReport=offline.Report;
                    if(offlineDefinition==null||offlineDefinition.ShowSummary)OfflineProgressApplied?.Invoke(offline.Report);
                }
                if(result.WasMigrated||offline.Report?.Applied==true)
                    PersistOfflineState();
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
        void OnApplicationPause(bool paused)
        {
            applicationPaused=paused;
            if(paused)BeginBackgroundInterval();else TryResumeOffline();
        }
        void OnApplicationFocus(bool focused)
        {
            applicationFocused=focused;
            if(!focused)BeginBackgroundInterval();else TryResumeOffline();
        }
        void BeginBackgroundInterval()
        {
            if(backgroundIntervalOpen)return;backgroundIntervalOpen=true;
            if(current!=null)current.LastApplicationPauseAtUtc=DateTime.UtcNow.ToString("O");
            SaveNow();
        }
        void TryResumeOffline()
        {
            if(!backgroundIntervalOpen||applicationPaused||!applicationFocused)return;
            backgroundIntervalOpen=false;
            if(current==null||(offlineDefinition!=null&&!offlineDefinition.AllowResume))return;
            current.LastApplicationResumeAtUtc=DateTime.UtcNow.ToString("O");
            var result=offlineService.Simulate(current,OfflineSimulationPolicy.From(offlineDefinition),false);
            if(!result.Success)return;current=result.Data;
            if(result.Report?.Applied==true)
            {
                mapper.Apply(current,manager,AreaForSlot);LastOfflineReport=result.Report;
                ApplyOfflineProgression(result.Report);
                if(offlineDefinition==null||offlineDefinition.ShowSummary)OfflineProgressApplied?.Invoke(result.Report);
                PersistOfflineState();
            }
        }
        void ApplyOfflineProgression(OfflineSimulationReport report)
        {
            if(report?.Applied!=true||journal==null)return;
            journal.ApplyOfflineProgress((float)report.Time.Effective.TotalHours,report.AnyExcellentWater,report.AnyExcellentWelfare);
            mapper.CaptureProgress(current,journal.Player,journal.Missions,journal.Codex,journal.Achievements);
        }
        void PersistOfflineState()
        {
            if(offlineDefinition!=null&&!offlineDefinition.SaveImmediately){MarkDirty();return;}
            var saveResult=service.Save(current);
            if(!saveResult.Success)
            {
                dirty=true;dirtyAt=Time.unscaledTime;
                SetStatus(SaveStatus.Dirty,$"Offline progress is pending save: {saveResult.Message}");
            }
        }
        void OnApplicationQuit()
        {
            if (current != null) current.LastSessionEndedAtUtc = DateTime.UtcNow.ToString("O");
            SaveNow();
        }
        void SetStatus(SaveStatus value, string message) { Status = value; LastMessage = message; StatusChanged?.Invoke(value, message); }
    }
}
