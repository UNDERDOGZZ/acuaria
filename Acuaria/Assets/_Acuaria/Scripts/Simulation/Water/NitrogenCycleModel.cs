using UnityEngine;

namespace Acuaria.Simulation.Water
{
    public sealed class NitrogenCycleModel
    {
        public WaterChemistryState Step(WaterChemistryState source, WaterChemistryDefinition definition,
            float simulatedSeconds)
            => Step(source, definition, simulatedSeconds, 1f);

        public WaterChemistryState Step(WaterChemistryState source, WaterChemistryDefinition definition,
            float simulatedSeconds, float biologicalEfficiency)
        {
            if (source == null || definition == null) return null;
            var next = source.Snapshot();
            if (!source.IsInitialized || !float.IsFinite(simulatedSeconds) || simulatedSeconds <= 0f) return next;

            var hours = Mathf.Min(simulatedSeconds / 3600f, 24f);
            var decomposedWaste = Mathf.Min(source.OrganicWaste,
                source.OrganicWaste * definition.WasteToAmmoniaRate * hours);
            var ammonia = source.AmmoniaMgPerLiter + decomposedWaste / definition.ReferenceVolumeLiters;
            var nitrite = source.NitriteMgPerLiter;
            var nitrate = source.NitrateMgPerLiter;

            var filterFactor = Mathf.Clamp(Safe(biologicalEfficiency), 0f, 2f);
            var ammoniaConverted = Mathf.Min(ammonia,
                ammonia * source.AmmoniaOxidizingBacteria * definition.AmmoniaConversionRate * hours * filterFactor);
            ammonia -= ammoniaConverted;
            nitrite += ammoniaConverted;

            var nitriteConverted = Mathf.Min(nitrite,
                nitrite * source.NitriteOxidizingBacteria * definition.NitriteConversionRate * hours * filterFactor);
            nitrite -= nitriteConverted;
            nitrate += nitriteConverted;

            var ammoniaBacteria = Grow(source.AmmoniaOxidizingBacteria, ammonia,
                definition.BacteriaGrowthRate, definition.BacteriaLossRate, hours);
            var nitriteBacteria = Grow(source.NitriteOxidizingBacteria, nitrite,
                definition.BacteriaGrowthRate, definition.BacteriaLossRate, hours);

            next.ApplyStep(ammonia, nitrite, nitrate, ammoniaBacteria, nitriteBacteria,
                source.OrganicWaste - decomposedWaste, simulatedSeconds, definition.TrendTolerance, definition);
            return next;
        }
        private static float Safe(float value) => float.IsFinite(value) ? value : 0f;

        private static float Grow(float bacteria, float resource, float growth, float loss, float hours)
        {
            var resourceFactor = resource <= 0f ? 0f : resource / (resource + 0.2f);
            return bacteria + bacteria * growth * resourceFactor * hours - bacteria * loss * (1f - resourceFactor) * hours;
        }
    }
}
