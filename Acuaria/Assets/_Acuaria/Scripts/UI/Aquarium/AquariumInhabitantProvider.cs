using System;
using System.Collections.Generic;
using Acuaria.Aquarium;
using Acuaria.Fish;
using UnityEngine;

namespace Acuaria.UI.Aquarium
{
    public sealed class AquariumInhabitantProvider : MonoBehaviour
    {
        [SerializeField] private FishSpawner2D fishSpawner;
        private readonly List<FishSpeciesDefinition> source = new(4);
        private readonly List<AquariumInhabitant> inhabitants = new(4);
        public IReadOnlyList<AquariumInhabitant> Inhabitants => inhabitants;
        public int TotalCount { get; private set; }
        public event Action PopulationChanged;

        public void Configure(FishSpawner2D spawner) => fishSpawner = spawner;

        private void OnEnable()
        {
            if (fishSpawner != null) fishSpawner.PopulationChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (fishSpawner != null) fishSpawner.PopulationChanged -= Refresh;
        }

        public void Refresh()
        {
            source.Clear();
            if (fishSpawner != null) fishSpawner.CopySpawnedSpecies(source);
            Rebuild(source);
        }

        public void Rebuild(IReadOnlyList<FishSpeciesDefinition> species)
        {
            inhabitants.Clear();
            TotalCount = 0;
            if (species != null)
            {
                for (var index = 0; index < species.Count; index++)
                {
                    var item = species[index];
                    if (item == null || !item.IsValid) continue;
                    TotalCount++;
                    var existing = Find(item.SpeciesId);
                    if (existing >= 0)
                    {
                        var current = inhabitants[existing];
                        inhabitants[existing] = new AquariumInhabitant(current.SpeciesId, current.DisplayName,
                            current.SwimmingZone, current.Count + 1);
                    }
                    else
                    {
                        inhabitants.Add(new AquariumInhabitant(item.SpeciesId,
                            string.IsNullOrWhiteSpace(item.DisplayName) ? item.SpeciesId : item.DisplayName,
                            ZoneLabel(item.SwimmingLevel), 1));
                    }
                }
            }
            PopulationChanged?.Invoke();
        }

        private int Find(string speciesId)
        {
            for (var index = 0; index < inhabitants.Count; index++)
                if (inhabitants[index].SpeciesId == speciesId) return index;
            return -1;
        }

        private static string ZoneLabel(SwimmingLevel level) => level switch
        {
            SwimmingLevel.Upper => "Zona superior",
            SwimmingLevel.Middle => "Zona media",
            SwimmingLevel.Lower => "Zona baja",
            _ => "Todas las zonas"
        };
    }
}
