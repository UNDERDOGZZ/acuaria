namespace Acuaria.Simulation.Water
{
    public enum WaterQualityStatus { Excellent, Good, Warning, Dangerous }
    public enum AquariumCycleStatus { Uncycled, Cycling, NearlyCycled, Cycled, Unstable }

    public readonly struct WaterQualityResult
    {
        public WaterQualityResult(WaterQualityStatus status, string explanation, float normalized)
        { Status = status; Explanation = explanation; Normalized = normalized; }
        public WaterQualityStatus Status { get; }
        public string Explanation { get; }
        public float Normalized { get; }
    }

    public static class AquariumCycleEvaluator
    {
        public static AquariumCycleStatus Evaluate(WaterChemistryState state, WaterChemistryDefinition definition)
        {
            if (state == null || definition == null || !state.IsInitialized) return AquariumCycleStatus.Uncycled;
            var bacteria = System.Math.Min(state.AmmoniaOxidizingBacteria, state.NitriteOxidizingBacteria);
            if (bacteria < 0.15f) return AquariumCycleStatus.Uncycled;
            if (state.AmmoniaMgPerLiter > definition.Thresholds.ammoniaDangerous ||
                state.NitriteMgPerLiter > definition.Thresholds.nitriteDangerous) return AquariumCycleStatus.Unstable;
            if (state.AmmoniaMgPerLiter > definition.Thresholds.ammoniaWarning ||
                state.NitriteMgPerLiter > definition.Thresholds.nitriteWarning) return AquariumCycleStatus.Cycling;
            if (bacteria < 0.5f || state.NitriteTrend == WaterParameterTrend.Falling) return AquariumCycleStatus.NearlyCycled;
            return AquariumCycleStatus.Cycled;
        }
    }

    public static class WaterQualityEvaluator
    {
        public static WaterQualityResult Evaluate(WaterChemistryState state, WaterChemistryDefinition definition,
            AquariumCycleStatus cycle)
        {
            if (state == null || definition == null || !state.IsInitialized)
                return new WaterQualityResult(WaterQualityStatus.Dangerous, "Datos del agua incompletos.", 0f);
            var thresholds = definition.Thresholds;
            if (state.AmmoniaMgPerLiter >= thresholds.ammoniaDangerous ||
                state.NitriteMgPerLiter >= thresholds.nitriteDangerous ||
                state.NitrateMgPerLiter >= thresholds.nitrateDangerous)
                return new WaterQualityResult(WaterQualityStatus.Dangerous, "La calidad del agua necesita atención prioritaria.", 0.15f);
            if (state.AmmoniaMgPerLiter >= thresholds.ammoniaWarning ||
                state.NitriteMgPerLiter >= thresholds.nitriteWarning ||
                state.NitrateMgPerLiter >= thresholds.nitrateWarning ||
                cycle is AquariumCycleStatus.Uncycled or AquariumCycleStatus.Unstable)
                return new WaterQualityResult(WaterQualityStatus.Warning, "El ciclo biológico aún no mantiene todos los valores seguros.", 0.45f);
            if (cycle != AquariumCycleStatus.Cycled)
                return new WaterQualityResult(WaterQualityStatus.Good, "El agua es segura y la colonia bacteriana sigue madurando.", 0.75f);
            return new WaterQualityResult(WaterQualityStatus.Excellent, "El agua está estable y el ciclo biológico funciona correctamente.", 1f);
        }
    }
}
