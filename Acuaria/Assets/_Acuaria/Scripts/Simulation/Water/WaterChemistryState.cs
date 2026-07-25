using System;
using UnityEngine;

namespace Acuaria.Simulation.Water
{
    public enum WaterParameterTrend { Falling, Stable, Rising }

    [Serializable]
    public sealed class WaterChemistryState
    {
        public string InstanceId { get; private set; }
        public float AmmoniaMgPerLiter { get; private set; }
        public float NitriteMgPerLiter { get; private set; }
        public float NitrateMgPerLiter { get; private set; }
        public float AmmoniaOxidizingBacteria { get; private set; }
        public float NitriteOxidizingBacteria { get; private set; }
        public float OrganicWaste { get; private set; }
        public double TotalSimulatedSeconds { get; private set; }
        public float LastSimulationStep { get; private set; }
        public bool IsInitialized { get; private set; }
        public uint Version { get; private set; }
        public WaterParameterTrend AmmoniaTrend { get; private set; } = WaterParameterTrend.Stable;
        public WaterParameterTrend NitriteTrend { get; private set; } = WaterParameterTrend.Stable;
        public WaterParameterTrend NitrateTrend { get; private set; } = WaterParameterTrend.Stable;

        public void Initialize(string id, WaterChemistryDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(id) || definition == null) throw new ArgumentException("Valid state ID and definition are required.");
            InstanceId = id;
            SetValues(definition.InitialAmmonia, definition.InitialNitrite, definition.InitialNitrate,
                definition.InitialAmmoniaBacteria, definition.InitialNitriteBacteria, 0f, definition);
            TotalSimulatedSeconds = 0d;
            LastSimulationStep = 0f;
            Version = 1;
            IsInitialized = true;
        }

        public void AddWaste(float amount, WaterChemistryDefinition definition) =>
            SetWaste(OrganicWaste + Safe(amount), definition);
        public void AddAmmonia(float amount, WaterChemistryDefinition definition) =>
            SetValues(AmmoniaMgPerLiter + Safe(amount), NitriteMgPerLiter, NitrateMgPerLiter,
                AmmoniaOxidizingBacteria, NitriteOxidizingBacteria, OrganicWaste, definition);
        public void SetDevelopmentValues(float ammonia, float nitrite, float nitrate, float ammoniaBacteria,
            float nitriteBacteria, float waste, WaterChemistryDefinition definition) =>
            SetValues(ammonia, nitrite, nitrate, ammoniaBacteria, nitriteBacteria, waste, definition);

        public void ApplyMaintenanceValues(float ammonia, float nitrite, float nitrate, float ammoniaBacteria,
            float nitriteBacteria, float waste, WaterChemistryDefinition definition)
        {
            var previousAmmonia = AmmoniaMgPerLiter;
            var previousNitrite = NitriteMgPerLiter;
            var previousNitrate = NitrateMgPerLiter;
            SetValues(ammonia, nitrite, nitrate, ammoniaBacteria, nitriteBacteria, waste, definition);
            AmmoniaTrend = Trend(previousAmmonia, AmmoniaMgPerLiter, definition.TrendTolerance);
            NitriteTrend = Trend(previousNitrite, NitriteMgPerLiter, definition.TrendTolerance);
            NitrateTrend = Trend(previousNitrate, NitrateMgPerLiter, definition.TrendTolerance);
        }

        public void ApplyStep(float ammonia, float nitrite, float nitrate, float ammoniaBacteria,
            float nitriteBacteria, float waste, float simulatedSeconds, float trendTolerance,
            WaterChemistryDefinition definition)
        {
            var previousAmmonia = AmmoniaMgPerLiter;
            var previousNitrite = NitriteMgPerLiter;
            var previousNitrate = NitrateMgPerLiter;
            SetValues(ammonia, nitrite, nitrate, ammoniaBacteria, nitriteBacteria, waste, definition);
            LastSimulationStep = Mathf.Max(0f, Safe(simulatedSeconds));
            TotalSimulatedSeconds += LastSimulationStep;
            AmmoniaTrend = Trend(previousAmmonia, AmmoniaMgPerLiter, trendTolerance);
            NitriteTrend = Trend(previousNitrite, NitriteMgPerLiter, trendTolerance);
            NitrateTrend = Trend(previousNitrate, NitrateMgPerLiter, trendTolerance);
        }

        public WaterChemistryState Snapshot()
        {
            var copy = new WaterChemistryState
            {
                InstanceId = InstanceId, AmmoniaMgPerLiter = AmmoniaMgPerLiter,
                NitriteMgPerLiter = NitriteMgPerLiter, NitrateMgPerLiter = NitrateMgPerLiter,
                AmmoniaOxidizingBacteria = AmmoniaOxidizingBacteria,
                NitriteOxidizingBacteria = NitriteOxidizingBacteria, OrganicWaste = OrganicWaste,
                TotalSimulatedSeconds = TotalSimulatedSeconds, LastSimulationStep = LastSimulationStep,
                IsInitialized = IsInitialized, Version = Version, AmmoniaTrend = AmmoniaTrend,
                NitriteTrend = NitriteTrend, NitrateTrend = NitrateTrend
            };
            return copy;
        }

        private void SetWaste(float waste, WaterChemistryDefinition definition)
        {
            OrganicWaste = Mathf.Clamp(Safe(waste), 0f, definition.MaximumWaste);
            Version++;
        }

        private void SetValues(float ammonia, float nitrite, float nitrate, float ammoniaBacteria,
            float nitriteBacteria, float waste, WaterChemistryDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            AmmoniaMgPerLiter = Mathf.Clamp(Safe(ammonia), 0f, definition.MaximumConcentration);
            NitriteMgPerLiter = Mathf.Clamp(Safe(nitrite), 0f, definition.MaximumConcentration);
            NitrateMgPerLiter = Mathf.Clamp(Safe(nitrate), 0f, definition.MaximumConcentration);
            AmmoniaOxidizingBacteria = Mathf.Clamp(Safe(ammoniaBacteria), 0f, definition.MaximumBacteria);
            NitriteOxidizingBacteria = Mathf.Clamp(Safe(nitriteBacteria), 0f, definition.MaximumBacteria);
            OrganicWaste = Mathf.Clamp(Safe(waste), 0f, definition.MaximumWaste);
            Version++;
        }

        private static WaterParameterTrend Trend(float previous, float current, float tolerance)
        {
            var delta = current - previous;
            if (Mathf.Abs(delta) <= Mathf.Max(0.000001f, tolerance)) return WaterParameterTrend.Stable;
            return delta > 0f ? WaterParameterTrend.Rising : WaterParameterTrend.Falling;
        }
        private static float Safe(float value) => float.IsFinite(value) ? Mathf.Max(0f, value) : 0f;
    }
}
