using System;
using Acuaria.Simulation.Water;
using Acuaria.Fish.Welfare;

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

        public static AquariumStatusResult Evaluate(AquariumDefinition definition, AquariumRuntimeState state,
            WaterQualityResult waterQuality)
        {
            var baseline = Evaluate(definition, state);
            var waterSeverity = waterQuality.Status switch
            {
                WaterQualityStatus.Dangerous => 3,
                WaterQualityStatus.Warning => 2,
                WaterQualityStatus.Good => 1,
                _ => 0
            };
            if (waterSeverity <= baseline.Severity) return baseline;
            return new AquariumStatusResult(waterSeverity == 3 ? AquariumStatus.Critical : AquariumStatus.Attention,
                waterQuality.Explanation, waterSeverity);
        }

        public static AquariumStatusResult Evaluate(AquariumDefinition definition,AquariumRuntimeState state,
            WaterQualityResult waterQuality,AquariumWelfareResult welfare)
        {
            var baseline=Evaluate(definition,state,waterQuality);
            var severity=welfare.Status switch{FishWelfareStatus.Poor=>3,FishWelfareStatus.Attention=>2,FishWelfareStatus.Good=>1,_=>0};
            if(severity<=baseline.Severity)return baseline;
            return new AquariumStatusResult(severity==3?AquariumStatus.Critical:AquariumStatus.Attention,
                $"Bienestar de los peces: {welfare.Status}",severity);
        }
    }
}
