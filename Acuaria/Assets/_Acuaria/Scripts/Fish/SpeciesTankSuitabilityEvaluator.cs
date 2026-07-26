using System.Collections.Generic;
using Acuaria.Fish.Care;

namespace Acuaria.Fish
{
    public enum SpeciesTankSuitability { Suitable, Caution, Unsuitable, InsufficientData }
    public readonly struct SpeciesTankSuitabilityResult
    {
        public readonly SpeciesTankSuitability Status;public readonly IReadOnlyList<string> Reasons,Recommendations;
        public SpeciesTankSuitabilityResult(SpeciesTankSuitability status,List<string> reasons,List<string> recommendations)
        {Status=status;Reasons=reasons;Recommendations=recommendations;}
    }
    public readonly struct SpeciesTankContext
    {
        public readonly float VolumeLitres,Temperature;public readonly int IntendedSameSpeciesCount;
        public readonly AquariumHabitatProfile Habitat;
        public SpeciesTankContext(float volume,float temperature,int count,AquariumHabitatProfile habitat)
        {VolumeLitres=volume;Temperature=temperature;IntendedSameSpeciesCount=count;Habitat=habitat;}
    }
    public sealed class SpeciesTankSuitabilityEvaluator
    {
        public SpeciesTankSuitabilityResult Evaluate(FishSpeciesDefinition species,SpeciesTankContext context)
        {
            var reasons=new List<string>();var advice=new List<string>();
            if(species==null||species.Care==null||!species.Care.IsValid||context.Habitat==null)
                return new SpeciesTankSuitabilityResult(SpeciesTankSuitability.InsufficientData,reasons,advice);
            var care=species.Care;var status=SpeciesTankSuitability.Suitable;
            if(context.Temperature<care.TemperatureRange.x||context.Temperature>care.TemperatureRange.y)
            {status=SpeciesTankSuitability.Unsuitable;reasons.Add("Temperatura fuera del rango recomendado.");advice.Add("Ajusta la temperatura gradualmente.");}
            var required=context.IntendedSameSpeciesCount>1?care.MinimumGroupVolume:care.MinimumIndividualVolume;
            if(context.VolumeLitres<required){status=SpeciesTankSuitability.Unsuitable;reasons.Add("Volumen orientativo insuficiente.");advice.Add($"Considera al menos {required:0} L.");}
            if(species.Social.SocialType!=FishSocialType.Solitary&&context.IntendedSameSpeciesCount<species.Social.RecommendedMinimum)
            {if(status==SpeciesTankSuitability.Suitable)status=SpeciesTankSuitability.Caution;reasons.Add("No permite el grupo social recomendado.");}
            if(!context.Habitat.Supports(care.SwimmingZone)){status=SpeciesTankSuitability.Unsuitable;reasons.Add("La zona de nado requerida no está disponible.");}
            if(care.NeedsHidingPlaces&&!context.Habitat.HidingPlaces){if(status==SpeciesTankSuitability.Suitable)status=SpeciesTankSuitability.Caution;reasons.Add("Faltan refugios.");}
            return new SpeciesTankSuitabilityResult(status,reasons,advice);
        }
    }

    public sealed class FishSpeciesViewModel
    {
        public FishSpeciesDefinition Species{get;}public FishDiscoveryState Discovery{get;}public SpeciesTankSuitabilityResult Suitability{get;}
        public string Title=>Species==null?string.Empty:Species.DisplayName;
        public string ScientificName=>Discovery>=FishDiscoveryState.Discovered?Species?.ScientificName:string.Empty;
        public bool ShowsFullProfile=>Discovery==FishDiscoveryState.Studied;
        public FishSpeciesViewModel(FishSpeciesDefinition species,FishDiscoveryState discovery,SpeciesTankSuitabilityResult suitability)
        {Species=species;Discovery=discovery;Suitability=suitability;}
    }

    public sealed class FishDiscoveryTracker
    {
        readonly Dictionary<string,FishDiscoveryState> states=new();
        public FishDiscoveryState Get(string id)=>!string.IsNullOrWhiteSpace(id)&&states.TryGetValue(id,out var state)?state:FishDiscoveryState.Hidden;
        public bool Advance(string id,FishDiscoveryState state)
        {if(string.IsNullOrWhiteSpace(id))return false;var current=Get(id);if(state<=current)return false;states[id]=state;return true;}
    }
}
