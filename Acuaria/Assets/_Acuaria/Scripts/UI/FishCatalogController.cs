using System;
using System.Collections.Generic;
using Acuaria.Fish;
using Acuaria.Fish.Care;
using UnityEngine;

namespace Acuaria.UI
{
    public sealed class FishCatalogController : MonoBehaviour
    {
        [SerializeField] FishSpeciesRegistry registry;
        [SerializeField] FishDiscoveryState defaultDiscovery = FishDiscoveryState.Discovered;
        [SerializeField] string[] studiedSpeciesIds = Array.Empty<string>();
        readonly FishDiscoveryTracker discovery = new();
        readonly List<FishSpeciesViewModel> visible = new();

        public IReadOnlyList<FishSpeciesViewModel> VisibleSpecies => visible;
        public FishSpeciesViewModel Selected { get; private set; }
        public event Action Changed;
        public event Action<FishSpeciesViewModel> SelectionChanged;

        public void Configure(FishSpeciesRegistry value, params string[] studiedIds)
        {
            registry = value;
            studiedSpeciesIds = studiedIds ?? Array.Empty<string>();
            InitializeDiscovery();
            Refresh();
        }

        void OnEnable()
        {
            InitializeDiscovery();
            Refresh();
        }

        void InitializeDiscovery()
        {
            if (registry == null) return;
            for (var i = 0; i < registry.Species.Count; i++)
            {
                var item = registry.Species[i];
                if (item != null) discovery.Advance(item.SpeciesId, defaultDiscovery);
            }
            for (var i = 0; i < studiedSpeciesIds.Length; i++)
                discovery.Advance(studiedSpeciesIds[i], FishDiscoveryState.Studied);
        }

        public void Refresh(FishCareDifficulty? difficulty = null, SwimmingLevel? zone = null, FishSocialType? social = null)
        {
            visible.Clear();
            if (registry != null)
            {
                var filtered = registry.Filter(difficulty, zone, social);
                for (var i = 0; i < filtered.Count; i++)
                {
                    var state = discovery.Get(filtered[i].SpeciesId);
                    if (state == FishDiscoveryState.Hidden) continue;
                    visible.Add(new FishSpeciesViewModel(filtered[i], state, default));
                }
            }
            Changed?.Invoke();
        }

        public bool SetDiscovery(string speciesId, FishDiscoveryState state)
        {
            var changed = registry != null && registry.FindById(speciesId) != null && discovery.Advance(speciesId, state);
            if (changed) Refresh();
            return changed;
        }

        public bool Select(string speciesId)
        {
            var species = registry?.FindById(speciesId);
            if (species == null) return false;
            var state = discovery.Get(speciesId);
            if (state == FishDiscoveryState.Hidden) return false;
            Selected = new FishSpeciesViewModel(species, state, default);
            SelectionChanged?.Invoke(Selected);
            return true;
        }
    }
}
