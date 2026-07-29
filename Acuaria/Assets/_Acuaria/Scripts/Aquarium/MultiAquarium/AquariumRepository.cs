using System;
using System.Collections.Generic;

namespace Acuaria.Aquarium.MultiAquarium
{
    public sealed class AquariumRepository
    {
        readonly Dictionary<string, AquariumInstance> byId = new(StringComparer.Ordinal);
        readonly List<AquariumInstance> ordered = new();
        public IReadOnlyList<AquariumInstance> All => ordered;
        public int Count => ordered.Count;
        public bool Register(AquariumInstance instance)
        {
            if (instance == null || string.IsNullOrWhiteSpace(instance.InstanceId) || byId.ContainsKey(instance.InstanceId)) return false;
            byId.Add(instance.InstanceId, instance); ordered.Add(instance); return true;
        }
        public AquariumInstance Find(string id) => !string.IsNullOrWhiteSpace(id) && byId.TryGetValue(id, out var value) ? value : null;
        public bool Remove(string id)
        {
            var value = Find(id); if (value == null) return false;
            byId.Remove(id); ordered.Remove(value); return true;
        }
        internal void Clear() { byId.Clear(); ordered.Clear(); }
    }
}
