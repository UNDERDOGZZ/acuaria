using System.Globalization;
using UnityEngine;

namespace Acuaria.Simulation.Water
{
    public sealed class WaterChemistryViewModel
    {
        public WaterChemistryViewModel(WaterChemistryState state, WaterChemistryDefinition definition)
        {
            State = state?.Snapshot();
            CycleStatus = AquariumCycleEvaluator.Evaluate(state, definition);
            Quality = WaterQualityEvaluator.Evaluate(state, definition, CycleStatus);
            AmmoniaText = Format(state?.AmmoniaMgPerLiter ?? 0f);
            NitriteText = Format(state?.NitriteMgPerLiter ?? 0f);
            NitrateText = Format(state?.NitrateMgPerLiter ?? 0f);
            AmmoniaTrendText = Trend(state?.AmmoniaTrend ?? WaterParameterTrend.Stable);
            NitriteTrendText = Trend(state?.NitriteTrend ?? WaterParameterTrend.Stable);
            NitrateTrendText = Trend(state?.NitrateTrend ?? WaterParameterTrend.Stable);
            QualityLabel = Quality.Status switch
            {
                WaterQualityStatus.Excellent => "Excelente",
                WaterQualityStatus.Good => "Buena",
                WaterQualityStatus.Warning => "Atención",
                _ => "Peligrosa"
            };
            CycleLabel = CycleStatus switch
            {
                AquariumCycleStatus.Uncycled => "Sin ciclar",
                AquariumCycleStatus.Cycling => "Ciclando",
                AquariumCycleStatus.NearlyCycled => "Casi ciclado",
                AquariumCycleStatus.Cycled => "Ciclado",
                _ => "Inestable"
            };
            ContextualTip = Tip(state, definition, Quality);
        }

        public WaterChemistryState State { get; }
        public WaterQualityResult Quality { get; }
        public AquariumCycleStatus CycleStatus { get; }
        public string AmmoniaText { get; }
        public string NitriteText { get; }
        public string NitrateText { get; }
        public string AmmoniaTrendText { get; }
        public string NitriteTrendText { get; }
        public string NitrateTrendText { get; }
        public string QualityLabel { get; }
        public string CycleLabel { get; }
        public string ContextualTip { get; }
        public float AmmoniaNormalized => Normalize(State?.AmmoniaMgPerLiter ?? 0f, 1f);
        public float NitriteNormalized => Normalize(State?.NitriteMgPerLiter ?? 0f, 1f);
        public float NitrateNormalized => Normalize(State?.NitrateMgPerLiter ?? 0f, 50f);

        public string DetailsText =>
            $"Calidad del agua: {QualityLabel}\nCiclo biológico: {CycleLabel}\n\n" +
            $"NH₃/NH₄  {AmmoniaText}  {AmmoniaTrendText}\n" +
            $"NO₂  {NitriteText}  {NitriteTrendText}\n" +
            $"NO₃  {NitrateText}  {NitrateTrendText}\n\n{ContextualTip}";

        private static string Format(float value) =>
            $"{Mathf.Max(0f, value).ToString(value < 1f ? "0.00" : "0.0", CultureInfo.InvariantCulture)} mg/L";
        private static string Trend(WaterParameterTrend trend) => trend switch
        {
            WaterParameterTrend.Rising => "↑ subiendo",
            WaterParameterTrend.Falling => "↓ bajando",
            _ => "→ estable"
        };
        private static float Normalize(float value, float maximum) => Mathf.Clamp01(value / maximum);
        private static string Tip(WaterChemistryState state, WaterChemistryDefinition definition,
            WaterQualityResult quality)
        {
            if (state == null || definition == null) return "La química del agua todavía no está disponible.";
            if (state.AmmoniaMgPerLiter >= definition.Thresholds.ammoniaWarning)
                return "El amoníaco está aumentando. Evita añadir más comida.";
            if (state.NitriteMgPerLiter >= definition.Thresholds.nitriteWarning)
                return "Los nitritos están elevados mientras la colonia bacteriana se desarrolla.";
            if (state.NitrateMgPerLiter >= definition.Thresholds.nitrateWarning)
                return "Los nitratos se acumulan y más adelante será necesario renovar parte del agua.";
            return quality.Explanation;
        }
    }
}
