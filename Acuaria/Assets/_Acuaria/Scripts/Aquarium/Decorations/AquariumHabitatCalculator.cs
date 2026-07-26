using System.Collections.Generic;
using Acuaria.Fish.Care;
using UnityEngine;

namespace Acuaria.Aquarium.Decorations
{
    public enum HabitatRating { Excellent, Good, Attention, Poor }

    public static class AquariumHabitatCalculator
    {
        public static AquariumHabitatProfile Calculate(IReadOnlyList<DecorationDefinition> installed)
        {
            var plants = 0f; var hiding = 0f; var consumed = 0f; var flow = 0f; var light = 0f; var complexity = 0f;
            if (installed != null)
                for (var i = 0; i < installed.Count; i++)
                {
                    if (installed[i] == null || !installed[i].IsValid) continue;
                    var c = installed[i].Contribution;
                    plants += c.PlantCoverage; hiding += c.HidingPlaces; consumed += c.OpenSpaceConsumed;
                    flow += c.FlowResistance; light += c.LightingCoverage; complexity += c.VisualComplexity;
                }
            return new AquariumHabitatProfile(Mathf.Clamp01(plants), hiding, Mathf.Clamp01(1f - consumed),
                Mathf.Clamp01(flow), Mathf.Clamp01(light), Mathf.Clamp01(complexity));
        }
    }
}
