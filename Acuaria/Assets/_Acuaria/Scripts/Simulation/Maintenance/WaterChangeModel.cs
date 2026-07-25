using Acuaria.Simulation.Water;
using UnityEngine;

namespace Acuaria.Simulation.Maintenance
{
    public enum WaterChangeRecommendation { Light, Recommended, Important, Large }
    public readonly struct WaterChangeOption
    {
        public readonly int Percentage; public readonly string DisplayName; public readonly string Description;
        public readonly WaterChangeRecommendation Recommendation;
        public WaterChangeOption(int value)
        {
            Percentage = value; DisplayName = $"{value}%";
            Recommendation = value switch { 10 => WaterChangeRecommendation.Light, 25 => WaterChangeRecommendation.Recommended, 40 => WaterChangeRecommendation.Important, _ => WaterChangeRecommendation.Large };
            Description = value switch { 10 => "Mantenimiento ligero", 25 => "Opción equilibrada recomendada", 40 => "Cambio importante", _ => "Cambio grande" };
        }
    }
    public readonly struct WaterChangeResult
    {
        public readonly bool IsValid; public readonly float Ammonia, Nitrite, Nitrate, Waste, AmmoniaBacteria, NitriteBacteria;
        public WaterChangeResult(bool valid, float ammonia, float nitrite, float nitrate, float waste, float aBacteria, float nBacteria)
        { IsValid = valid; Ammonia = ammonia; Nitrite = nitrite; Nitrate = nitrate; Waste = waste; AmmoniaBacteria = aBacteria; NitriteBacteria = nBacteria; }
    }
    public sealed class WaterChangeModel
    {
        public WaterChangeResult Calculate(WaterChemistryState state, AquariumMaintenanceDefinition definition, int percentage)
        {
            if (state == null || definition == null || !definition.IsAllowed(percentage)) return default;
            var remaining = 1f - percentage / 100f;
            var wasteRemaining = 1f - percentage / 100f * definition.WasteReductionFactor;
            return new WaterChangeResult(true, Safe(state.AmmoniaMgPerLiter * remaining),
                Safe(state.NitriteMgPerLiter * remaining), Safe(state.NitrateMgPerLiter * remaining),
                Safe(state.OrganicWaste * wasteRemaining), state.AmmoniaOxidizingBacteria,
                state.NitriteOxidizingBacteria);
        }
        private static float Safe(float value) => float.IsFinite(value) ? Mathf.Max(0f, value) : 0f;
    }
}
