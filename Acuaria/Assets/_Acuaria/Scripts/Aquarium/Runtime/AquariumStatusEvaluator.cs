using System;

namespace Acuaria.Aquarium
{
    public static class AquariumStatusEvaluator
    {
        private const float SlightTemperatureMargin = 1.5f;

        public static AquariumStatusResult Evaluate(AquariumDefinition definition, AquariumRuntimeState state)
        {
            if (definition == null || state == null || !definition.IsValid || !state.IsInitialized)
                return new AquariumStatusResult(AquariumStatus.Critical, "Datos del acuario incompletos", 3);

            var temperature = float.IsFinite(state.CurrentTemperature) ? state.CurrentTemperature : 100f;
            var distance = temperature < definition.TargetTemperatureMin
                ? definition.TargetTemperatureMin - temperature
                : Math.Max(0f, temperature - definition.TargetTemperatureMax);
            var overCapacity = Math.Max(0, state.CurrentFishCount - definition.RecommendedFishCapacity);

            if (distance > SlightTemperatureMargin || overCapacity > 1)
                return new AquariumStatusResult(AquariumStatus.Critical, "Necesita atención prioritaria", 3);
            if (distance > 0f || overCapacity == 1)
                return new AquariumStatusResult(AquariumStatus.Attention, "Revisa temperatura o población", 2);
            if (state.CurrentFishCount == definition.RecommendedFishCapacity && definition.RecommendedFishCapacity > 0)
                return new AquariumStatusResult(AquariumStatus.Good, "Parámetros estables", 1);
            return new AquariumStatusResult(AquariumStatus.Excellent, "Todo se ve excelente", 0);
        }
    }
}
