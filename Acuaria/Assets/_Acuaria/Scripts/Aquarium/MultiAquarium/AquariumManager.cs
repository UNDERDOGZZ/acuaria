using System;
using System.Collections.Generic;
using UnityEngine;

namespace Acuaria.Aquarium.MultiAquarium
{
    [DefaultExecutionOrder(-1200)]
    public sealed class AquariumManager : MonoBehaviour
    {
        public static AquariumManager Instance { get; private set; }
        readonly AquariumRepository repository = new();
        readonly AquariumContext context = new();
        AquariumFactory factory;
        public AquariumRepository Repository => repository;
        public AquariumContext Context => context;
        public AquariumInstance ActiveAquarium => context.Active;
        public IReadOnlyList<AquariumInstance> Aquariums => repository.All;
        public event Action<AquariumInstance> OnAquariumCreated, OnAquariumRemoved, OnAquariumActivated, OnAquariumDeactivated;
        public event Action<AquariumInstance, AquariumInstance> OnActiveAquariumChanged;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this; factory ??= new AquariumFactory();
        }
        public void ConfigureFactory(AquariumFactory value) => factory = value ?? throw new ArgumentNullException(nameof(value));
        public AquariumInstance CreateAquarium(AquariumDefinition definition, string id = null, string name = null)
        {
            factory ??= new AquariumFactory(); var instance = factory.Create(definition, id, name);
            if (!repository.Register(instance)) throw new InvalidOperationException($"Aquarium '{instance.InstanceId}' is already registered.");
            OnAquariumCreated?.Invoke(instance); if (ActiveAquarium == null) Activate(instance.InstanceId); return instance;
        }
        public AquariumInstance Find(string id) => repository.Find(id);
        public bool Activate(string id)
        {
            return ActivateInternal(id, true);
        }
        public bool RestoreActiveAquarium(string id) => ActivateInternal(id, false);
        bool ActivateInternal(string id, bool recordActivation)
        {
            var next = repository.Find(id); if (next == null || ReferenceEquals(next, ActiveAquarium)) return false;
            var previous = ActiveAquarium; if (previous != null) { previous.RuntimeState.SetFocused(false); OnAquariumDeactivated?.Invoke(previous); }
            next.RuntimeState.SetFocused(true); if(recordActivation) next.StatisticsState.Activate(); context.SetActive(next);
            OnAquariumActivated?.Invoke(next); OnActiveAquariumChanged?.Invoke(previous, next); return true;
        }
        public bool ActivateNext(int direction)
        {
            if (direction == 0 || repository.Count < 2 || ActiveAquarium == null) return false;
            var current = -1;
            for (var i = 0; i < repository.All.Count; i++)
                if (ReferenceEquals(repository.All[i], ActiveAquarium)) { current = i; break; }
            if (current < 0) return false;
            var next = current + Math.Sign(direction);
            return next >= 0 && next < repository.All.Count && Activate(repository.All[next].InstanceId);
        }
        public bool RemoveAquarium(string id)
        {
            var target = repository.Find(id); if (target == null || repository.Count <= 1) return false;
            var wasActive = ReferenceEquals(target, ActiveAquarium);
            if (!repository.Remove(id)) return false;
            OnAquariumRemoved?.Invoke(target); if (wasActive) Activate(repository.All[0].InstanceId); return true;
        }
        public void TickInactive(double seconds)
        {
            foreach (var aquarium in repository.All) if (!ReferenceEquals(aquarium, ActiveAquarium)) aquarium.TickInactive(seconds);
        }
        public void ResetForLoad()
        {
            context.Clear();
            repository.Clear();
        }
        void OnDestroy() { if (Instance == this) Instance = null; }
    }
}
