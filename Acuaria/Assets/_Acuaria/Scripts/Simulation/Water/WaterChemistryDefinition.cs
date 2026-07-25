using System.Collections.Generic;
using UnityEngine;

namespace Acuaria.Simulation.Water
{
    [CreateAssetMenu(menuName = "Acuaria/Simulation/Water Chemistry", fileName = "WaterChemistryDefinition")]
    public sealed class WaterChemistryDefinition : ScriptableObject
    {
        [SerializeField] private string chemistryId = "starter-chemistry";
        [SerializeField, Min(1f)] private float referenceVolumeLiters = 50f;
        [SerializeField, Min(0f)] private float initialAmmonia;
        [SerializeField, Min(0f)] private float initialNitrite;
        [SerializeField, Min(0f)] private float initialNitrate = 7f;
        [SerializeField, Range(0f, 1f)] private float initialAmmoniaBacteria = 0.55f;
        [SerializeField, Range(0f, 1f)] private float initialNitriteBacteria = 0.55f;
        [SerializeField, Min(0.01f)] private float maximumBacteria = 1f;
        [SerializeField, Min(0f)] private float ammoniaConversionRate = 0.1f;
        [SerializeField, Min(0f)] private float nitriteConversionRate = 0.08f;
        [SerializeField, Min(0f)] private float bacteriaGrowthRate = 0.012f;
        [SerializeField, Min(0f)] private float bacteriaLossRate = 0.001f;
        [SerializeField, Min(0f)] private float wastePerFishPerHour = 0.08f;
        [SerializeField, Min(0f)] private float wastePerExpiredFood = 0.12f;
        [SerializeField, Min(0f)] private float wastePerConsumedFood = 0.01f;
        [SerializeField, Min(0f)] private float wasteToAmmoniaRate = 0.18f;
        [SerializeField, Min(0.1f)] private float maximumConcentration = 200f;
        [SerializeField, Min(0.1f)] private float maximumWaste = 1000f;
        [SerializeField, Min(0.05f)] private float simulationIntervalSeconds = 1f;
        [SerializeField, Min(0.01f)] private float timeMultiplier = 60f;
        [SerializeField, Range(1, 20)] private int maximumTicksPerFrame = 5;
        [SerializeField, Min(0.0001f)] private float trendTolerance = 0.001f;
        [SerializeField] private WaterQualityThresholds thresholds = WaterQualityThresholds.Default;

        public string ChemistryId => chemistryId;
        public float ReferenceVolumeLiters => referenceVolumeLiters;
        public float InitialAmmonia => initialAmmonia;
        public float InitialNitrite => initialNitrite;
        public float InitialNitrate => initialNitrate;
        public float InitialAmmoniaBacteria => initialAmmoniaBacteria;
        public float InitialNitriteBacteria => initialNitriteBacteria;
        public float MaximumBacteria => maximumBacteria;
        public float AmmoniaConversionRate => ammoniaConversionRate;
        public float NitriteConversionRate => nitriteConversionRate;
        public float BacteriaGrowthRate => bacteriaGrowthRate;
        public float BacteriaLossRate => bacteriaLossRate;
        public float WastePerFishPerHour => wastePerFishPerHour;
        public float WastePerExpiredFood => wastePerExpiredFood;
        public float WastePerConsumedFood => wastePerConsumedFood;
        public float WasteToAmmoniaRate => wasteToAmmoniaRate;
        public float MaximumConcentration => maximumConcentration;
        public float MaximumWaste => maximumWaste;
        public float SimulationIntervalSeconds => simulationIntervalSeconds;
        public float TimeMultiplier => timeMultiplier;
        public int MaximumTicksPerFrame => maximumTicksPerFrame;
        public float TrendTolerance => trendTolerance;
        public WaterQualityThresholds Thresholds => thresholds;

        public void Configure(string id, float volume, Vector3 initial, Vector2 bacteria, Vector2 conversions,
            Vector2 bacteriaRates, Vector3 wasteRates, Vector2 maxima, Vector2 clock, int maxTicks,
            float tolerance, WaterQualityThresholds qualityThresholds)
        {
            chemistryId = id;
            referenceVolumeLiters = Positive(volume, 50f);
            initialAmmonia = NonNegative(initial.x);
            initialNitrite = NonNegative(initial.y);
            initialNitrate = NonNegative(initial.z);
            maximumBacteria = 1f;
            initialAmmoniaBacteria = Mathf.Clamp(Valid(bacteria.x), 0f, maximumBacteria);
            initialNitriteBacteria = Mathf.Clamp(Valid(bacteria.y), 0f, maximumBacteria);
            ammoniaConversionRate = NonNegative(conversions.x);
            nitriteConversionRate = NonNegative(conversions.y);
            bacteriaGrowthRate = NonNegative(bacteriaRates.x);
            bacteriaLossRate = NonNegative(bacteriaRates.y);
            wastePerFishPerHour = NonNegative(wasteRates.x);
            wastePerExpiredFood = NonNegative(wasteRates.y);
            wastePerConsumedFood = NonNegative(wasteRates.z);
            maximumConcentration = Positive(maxima.x, 200f);
            maximumWaste = Positive(maxima.y, 1000f);
            simulationIntervalSeconds = Positive(clock.x, 1f);
            timeMultiplier = Positive(clock.y, 60f);
            maximumTicksPerFrame = Mathf.Clamp(maxTicks, 1, 20);
            trendTolerance = Positive(tolerance, 0.001f);
            thresholds = qualityThresholds.Sanitized();
        }

        public bool Validate(List<string> issues)
        {
            if (issues == null) return false;
            if (string.IsNullOrWhiteSpace(chemistryId)) issues.Add("ChemistryId is required.");
            if (referenceVolumeLiters <= 0f || !float.IsFinite(referenceVolumeLiters)) issues.Add("ReferenceVolumeLiters must be positive.");
            if (maximumBacteria <= 0f || !float.IsFinite(maximumBacteria)) issues.Add("MaximumBacteria must be positive.");
            if (initialAmmoniaBacteria < 0f || initialAmmoniaBacteria > maximumBacteria) issues.Add("InitialAmmoniaBacteria is outside capacity.");
            if (initialNitriteBacteria < 0f || initialNitriteBacteria > maximumBacteria) issues.Add("InitialNitriteBacteria is outside capacity.");
            if (simulationIntervalSeconds <= 0f || timeMultiplier <= 0f) issues.Add("Simulation clock values must be positive.");
            if (maximumConcentration <= 0f || maximumWaste <= 0f) issues.Add("Numeric maxima must be positive.");
            if (!thresholds.IsOrdered) issues.Add("Water quality thresholds must be ordered.");
            return issues.Count == 0;
        }

        private void OnValidate() => Configure(chemistryId, referenceVolumeLiters,
            new Vector3(initialAmmonia, initialNitrite, initialNitrate),
            new Vector2(initialAmmoniaBacteria, initialNitriteBacteria),
            new Vector2(ammoniaConversionRate, nitriteConversionRate),
            new Vector2(bacteriaGrowthRate, bacteriaLossRate),
            new Vector3(wastePerFishPerHour, wastePerExpiredFood, wastePerConsumedFood),
            new Vector2(maximumConcentration, maximumWaste),
            new Vector2(simulationIntervalSeconds, timeMultiplier), maximumTicksPerFrame, trendTolerance, thresholds);

        private static float Valid(float value) => float.IsFinite(value) ? value : 0f;
        private static float NonNegative(float value) => Mathf.Max(0f, Valid(value));
        private static float Positive(float value, float fallback) => float.IsFinite(value) && value > 0f ? value : fallback;
    }

    [System.Serializable]
    public struct WaterQualityThresholds
    {
        public float ammoniaWarning;
        public float ammoniaDangerous;
        public float nitriteWarning;
        public float nitriteDangerous;
        public float nitrateWarning;
        public float nitrateDangerous;
        public bool IsOrdered => ammoniaWarning >= 0f && ammoniaDangerous >= ammoniaWarning &&
                                 nitriteWarning >= 0f && nitriteDangerous >= nitriteWarning &&
                                 nitrateWarning >= 0f && nitrateDangerous >= nitrateWarning;
        public static WaterQualityThresholds Default => new()
        {
            ammoniaWarning = 0.1f, ammoniaDangerous = 0.5f,
            nitriteWarning = 0.1f, nitriteDangerous = 0.5f,
            nitrateWarning = 25f, nitrateDangerous = 50f
        };
        public WaterQualityThresholds Sanitized()
        {
            ammoniaWarning = Mathf.Max(0f, Safe(ammoniaWarning));
            ammoniaDangerous = Mathf.Max(ammoniaWarning, Safe(ammoniaDangerous));
            nitriteWarning = Mathf.Max(0f, Safe(nitriteWarning));
            nitriteDangerous = Mathf.Max(nitriteWarning, Safe(nitriteDangerous));
            nitrateWarning = Mathf.Max(0f, Safe(nitrateWarning));
            nitrateDangerous = Mathf.Max(nitrateWarning, Safe(nitrateDangerous));
            return this;
        }
        private static float Safe(float value) => float.IsFinite(value) ? value : 0f;
    }
}
