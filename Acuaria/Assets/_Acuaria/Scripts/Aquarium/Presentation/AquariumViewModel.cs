using System;
using System.Collections.Generic;
using System.Text;

namespace Acuaria.Aquarium
{
    public sealed class AquariumViewModel
    {
        public AquariumViewModel(AquariumDefinition definition, AquariumRuntimeState state,
            IReadOnlyList<AquariumInhabitant> inhabitants)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (state == null) throw new ArgumentNullException(nameof(state));
            DisplayName = definition.DisplayName;
            VolumeText = $"{MathF.Round(definition.NominalVolumeLitres):0} L";
            TemperatureText = $"{MathF.Round(state.CurrentTemperature):0} °C";
            FishCountText = $"{state.CurrentFishCount} peces";
            CapacityText = $"{state.CurrentFishCount} de {definition.RecommendedFishCapacity} peces";
            TemperatureRangeText =
                $"{MathF.Round(definition.TargetTemperatureMin):0}–{MathF.Round(definition.TargetTemperatureMax):0} °C";
            EducationTip = definition.EducationTip;
            Status = AquariumStatusEvaluator.Evaluate(definition, state);
            StatusLabel = AquariumUIText.StatusLabel(Status.Status);
            StatusDescription = Status.Message;
            InhabitantsText = FormatInhabitants(inhabitants);
        }

        public string DisplayName { get; }
        public string VolumeText { get; }
        public string TemperatureText { get; }
        public string FishCountText { get; }
        public string CapacityText { get; }
        public string TemperatureRangeText { get; }
        public string StatusLabel { get; }
        public string StatusDescription { get; }
        public string EducationTip { get; }
        public string InhabitantsText { get; }
        public AquariumStatusResult Status { get; }

        private static string FormatInhabitants(IReadOnlyList<AquariumInhabitant> inhabitants)
        {
            if (inhabitants == null || inhabitants.Count == 0) return AquariumUIText.NoInhabitants;
            var builder = new StringBuilder(96);
            for (var index = 0; index < inhabitants.Count; index++)
            {
                var inhabitant = inhabitants[index];
                if (index > 0) builder.Append('\n');
                builder.Append("• ").Append(inhabitant.DisplayName).Append(" x").Append(inhabitant.Count)
                    .Append("  ·  ").Append(inhabitant.SwimmingZone);
            }
            return builder.ToString();
        }
    }
}
