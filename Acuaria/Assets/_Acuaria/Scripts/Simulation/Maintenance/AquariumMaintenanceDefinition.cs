using System;
using UnityEngine;

namespace Acuaria.Simulation.Maintenance
{
    [CreateAssetMenu(menuName = "Acuaria/Maintenance Definition")]
    public sealed class AquariumMaintenanceDefinition : ScriptableObject
    {
        [SerializeField] private string maintenanceId = "starter-maintenance";
        [SerializeField] private int[] allowedPercentages = { 10, 25, 40, 50 };
        [SerializeField] private int recommendedPercentage = 25;
        [SerializeField, Min(0f)] private float cooldownSeconds = 4f;
        [SerializeField, Range(0f, 1f)] private float wasteReductionFactor = 0.8f;
        [SerializeField, Min(0.01f)] private float drainDuration = 1.2f;
        [SerializeField, Min(0.01f)] private float refillDuration = 1.2f;
        [SerializeField, Min(0.01f)] private float stabilizationDuration = 0.8f;
        [SerializeField, Min(0f)] private float nitrateRecommendationThreshold = 25f;
        [SerializeField, Min(0f)] private float wasteRecommendationThreshold = 20f;

        public string MaintenanceId => maintenanceId;
        public int[] AllowedPercentages => allowedPercentages;
        public int RecommendedPercentage => recommendedPercentage;
        public float CooldownSeconds => cooldownSeconds;
        public float WasteReductionFactor => wasteReductionFactor;
        public float DrainDuration => drainDuration;
        public float RefillDuration => refillDuration;
        public float StabilizationDuration => stabilizationDuration;
        public float NitrateRecommendationThreshold => nitrateRecommendationThreshold;
        public float WasteRecommendationThreshold => wasteRecommendationThreshold;
        public bool IsValid => Validate();

        public void Configure(string id, int[] percentages, int recommended, float cooldown, float wasteFactor,
            Vector3 durations, Vector2 thresholds)
        {
            maintenanceId = id?.Trim();
            allowedPercentages = percentages == null ? Array.Empty<int>() : (int[])percentages.Clone();
            Array.Sort(allowedPercentages);
            recommendedPercentage = recommended;
            cooldownSeconds = Safe(cooldown);
            wasteReductionFactor = Mathf.Clamp01(Safe(wasteFactor));
            drainDuration = Mathf.Max(0.01f, Safe(durations.x));
            refillDuration = Mathf.Max(0.01f, Safe(durations.y));
            stabilizationDuration = Mathf.Max(0.01f, Safe(durations.z));
            nitrateRecommendationThreshold = Safe(thresholds.x);
            wasteRecommendationThreshold = Safe(thresholds.y);
        }

        public bool IsAllowed(int percentage) =>
            percentage > 0 && percentage <= 50 && Array.IndexOf(allowedPercentages, percentage) >= 0;

        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(maintenanceId) || allowedPercentages == null ||
                allowedPercentages.Length == 0 || !IsAllowed(recommendedPercentage)) return false;
            for (var i = 0; i < allowedPercentages.Length; i++)
                if (allowedPercentages[i] <= 0 || allowedPercentages[i] > 50) return false;
            return drainDuration > 0f && refillDuration > 0f && stabilizationDuration > 0f;
        }

        private static float Safe(float value) => float.IsFinite(value) ? Mathf.Max(0f, value) : 0f;
    }
}
