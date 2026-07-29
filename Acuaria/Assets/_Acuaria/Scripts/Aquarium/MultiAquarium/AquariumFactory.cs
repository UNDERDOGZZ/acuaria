using System;
using Acuaria.Simulation.Maintenance;
using Acuaria.Simulation.Water;

namespace Acuaria.Aquarium.MultiAquarium
{
    public sealed class AquariumFactory
    {
        readonly WaterChemistryDefinition waterDefinition;
        int sequence;
        public AquariumFactory(WaterChemistryDefinition chemistry = null) => waterDefinition = chemistry;
        public AquariumInstance Create(AquariumDefinition definition, string instanceId = null, string displayName = null)
        {
            if (definition == null || !definition.IsValid) throw new ArgumentException("A valid aquarium definition is required.");
            var id = string.IsNullOrWhiteSpace(instanceId) ? $"aquarium-{++sequence}" : instanceId.Trim();
            var runtime = new AquariumRuntimeState(); runtime.Initialize(id, definition);
            var water = new WaterChemistryState();
            if (waterDefinition != null) water.Initialize($"{id}-water", waterDefinition);
            var maintenance = new AquariumMaintenanceState(); maintenance.Initialize($"{id}-maintenance");
            var instance = new AquariumInstance(id, string.IsNullOrWhiteSpace(displayName) ? definition.DisplayName : displayName,
                definition, runtime, definition.HabitatProfile, water, maintenance);
            instance.DecorationCollection.Replace(definition.DecorationPlacements);
            return instance;
        }
    }
}
