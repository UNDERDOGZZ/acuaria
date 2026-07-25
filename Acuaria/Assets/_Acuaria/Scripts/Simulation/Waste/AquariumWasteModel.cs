using UnityEngine;
using Acuaria.Simulation.Water;

namespace Acuaria.Simulation.Waste
{
    public static class AquariumWasteModel
    {
        public static float FishWaste(int fishCount, float simulatedHours, WaterChemistryDefinition definition)
        {
            if (definition == null || fishCount <= 0 || !float.IsFinite(simulatedHours) || simulatedHours <= 0f) return 0f;
            return Mathf.Max(0f, fishCount * definition.WastePerFishPerHour * simulatedHours);
        }

        public static float ExpiredFoodWaste(int units, WaterChemistryDefinition definition) =>
            definition == null || units <= 0 ? 0f : units * definition.WastePerExpiredFood;

        public static float ConsumedFoodWaste(int units, WaterChemistryDefinition definition) =>
            definition == null || units <= 0 ? 0f : units * definition.WastePerConsumedFood;

        public static float ToConcentration(float mass, float volumeLiters) =>
            !float.IsFinite(mass) || !float.IsFinite(volumeLiters) || mass <= 0f || volumeLiters <= 0f
                ? 0f : mass / volumeLiters;
    }
}
