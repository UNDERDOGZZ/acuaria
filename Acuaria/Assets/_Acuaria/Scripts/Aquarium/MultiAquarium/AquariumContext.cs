using System;

namespace Acuaria.Aquarium.MultiAquarium
{
    public sealed class AquariumContext
    {
        public AquariumInstance ActiveAquarium { get; private set; }
        public AquariumInstance Active => ActiveAquarium;
        public string ActiveAquariumId => ActiveAquarium?.InstanceId;
        public bool HasActiveAquarium => ActiveAquarium != null;
        public event Action<AquariumInstance, AquariumInstance> OnActiveAquariumChanging;
        public event Action<AquariumInstance, AquariumInstance> OnActiveAquariumChanged;
        public event Action<AquariumInstance, AquariumInstance> ActiveChanged
        {
            add => OnActiveAquariumChanged += value;
            remove => OnActiveAquariumChanged -= value;
        }
        public bool TryGetActiveAquarium(out AquariumInstance aquarium)
        {
            aquarium = ActiveAquarium;
            return aquarium != null;
        }
        internal bool SetActive(AquariumInstance next)
        {
            if (next == null || !next.IsInitialized || ReferenceEquals(ActiveAquarium, next)) return false;
            var previous = ActiveAquarium;
            OnActiveAquariumChanging?.Invoke(previous, next);
            if (previous != null) previous.IsActive = false;
            ActiveAquarium = next;
            next.IsActive = true;
            OnActiveAquariumChanged?.Invoke(previous, next);
            return true;
        }
        public bool SetActiveAquarium(AquariumInstance next) => SetActive(next);
        internal void Clear()
        {
            if (ActiveAquarium != null) ActiveAquarium.IsActive = false;
            ActiveAquarium = null;
        }
    }
}
