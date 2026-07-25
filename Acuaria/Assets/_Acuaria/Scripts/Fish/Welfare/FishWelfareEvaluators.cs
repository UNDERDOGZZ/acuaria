using System.Collections.Generic;
using Acuaria.Fish.Care;
using Acuaria.Fish.Compatibility;
using Acuaria.Simulation.Water;
using UnityEngine;
namespace Acuaria.Fish.Welfare
{
    public sealed class FishWelfareEvaluator
    {
        public FishWelfareEvaluationResult Evaluate(FishSpeciesDefinition species,FishWelfareContext context,FishWelfareDefinition definition)
        {
            if(species==null||species.Care==null||species.Social==null||definition==null||!definition.IsValid)return default;
            var issues=new List<string>(7);var recommendations=new List<string>(3);var care=species.Care;
            var distance=context.Temperature<care.TemperatureRange.x?care.TemperatureRange.x-context.Temperature:Mathf.Max(0,context.Temperature-care.TemperatureRange.y);
            var temp=distance<=0?100:distance<=1?75:Mathf.Max(10,70-distance*20);if(temp<100)Add(issues,recommendations,"Temperatura fuera del rango recomendado",$"Ajusta gradualmente a {care.TemperatureRange.x:0}–{care.TemperatureRange.y:0} °C.");
            var required=Mathf.Max(care.MinimumIndividualVolume,context.SameSpeciesCount>1?care.MinimumGroupVolume:0);var volume=required<=0?100:Mathf.Clamp01(context.Volume/required)*100;
            if(context.Stocking>=AquariumStockingStatus.Crowded)volume=Mathf.Min(volume,context.Stocking==AquariumStockingStatus.Overcrowded?35:60);
            if(volume<90)Add(issues,recommendations,"Espacio disponible insuficiente","Reduce ocupación o usa un acuario de mayor volumen.");
            var min=species.Social.SocialType==FishSocialType.Solitary?1:Mathf.Max(1,species.Social.RecommendedMinimum);
            var social=species.Social.SocialType==FishSocialType.Solitary?(context.SameSpeciesCount<=species.Social.ProvisionalMaximum?100:45):Mathf.Clamp01(context.SameSpeciesCount/(float)min)*100;
            if(social<90)Add(issues,recommendations,"Grupo social insuficiente",$"Esta especie se beneficia de un grupo de {min}.");
            var water=WaterScore(context.WaterQuality,care.WaterSensitivity);if(water<80)Add(issues,recommendations,"Calidad del agua inadecuada","Revisa amoníaco, nitritos y nitratos.");
            var feed=Mathf.Lerp(35,100,Mathf.Clamp01(context.Satiety/.55f));if(context.Satiety>.95f){feed=80;Add(issues,recommendations,"Saciedad muy alta","Ofrece porciones pequeñas y espera a que terminen de comer.");}
            var compatibility=context.Compatibility switch{FishCompatibilityStatus.Compatible=>100,FishCompatibilityStatus.Caution=>65,_=>25};
            if(compatibility<90)Add(issues,recommendations,"Compatibilidad requiere atención","Revisa actividad, zona y territorialidad de las especies.");
            var zone=context.Habitat!=null&&context.Habitat.Supports(care.SwimmingZone)?100:30;if(zone<90)Add(issues,recommendations,"Zona de nado no disponible","Proporciona acceso a su zona de nado preferida.");
            var total=temp*definition.TemperatureWeight+volume*definition.VolumeWeight+social*definition.SocialWeight+water*definition.WaterWeight+
                feed*definition.FeedingWeight+compatibility*definition.CompatibilityWeight+zone*definition.ZoneWeight;
            return new FishWelfareEvaluationResult(true,Mathf.Clamp(total/definition.WeightSum,0,100),temp,volume,social,water,feed,compatibility,zone,issues,recommendations);
        }
        static float WaterScore(WaterQualityStatus status,FishWaterSensitivity sensitivity)
        {var baseScore=status switch{WaterQualityStatus.Excellent=>100,WaterQualityStatus.Good=>85,WaterQualityStatus.Warning=>55,_=>15};
         var factor=sensitivity==FishWaterSensitivity.Sensitive?1.25f:sensitivity==FishWaterSensitivity.Hardy?.8f:1;return Mathf.Clamp(100-(100-baseScore)*factor,0,100);}
        static void Add(List<string> issues,List<string> recommendations,string issue,string recommendation){issues.Add(issue);if(recommendations.Count<3)recommendations.Add(recommendation);}
    }
    public sealed class FishWelfareSimulationModel
    {
        public void Step(FishWelfareState state,FishWelfareEvaluationResult evaluation,FishWelfareDefinition definition,float hours)
        {if(state==null||!state.IsInitialized||!evaluation.IsValid||definition==null||hours<=0||!float.IsFinite(hours))return;
         var delta=evaluation.OverallScore-state.CurrentScore;if(Mathf.Abs(delta)<.01f){state.Apply(state.CurrentScore,evaluation.OverallScore,hours,evaluation.ActiveIssues);return;}
         var speed=delta>0?definition.ImprovementPerHour:definition.DeteriorationPerHour;
         var next=Mathf.MoveTowards(state.CurrentScore,evaluation.OverallScore,speed*hours);state.Apply(next,evaluation.OverallScore,hours,evaluation.ActiveIssues);}
    }
    public readonly struct AquariumWelfareResult
    {public readonly float Score;public readonly FishWelfareStatus Status;public readonly string Recommendation;
     public AquariumWelfareResult(float score,FishWelfareStatus status,string recommendation){Score=score;Status=status;Recommendation=recommendation;}}
    public static class AquariumWelfareEvaluator
    {
        public static AquariumWelfareResult Evaluate(IReadOnlyList<FishWelfareState> states)
        {if(states==null||states.Count==0)return new AquariumWelfareResult(100,FishWelfareStatus.Excellent,"Añade habitantes compatibles cuando el acuario esté preparado.");
         float sum=0f,worst=100f;for(var i=0;i<states.Count;i++){if(states[i]==null)continue;sum+=states[i].CurrentScore;worst=Mathf.Min(worst,states[i].CurrentScore);}
         var score=Mathf.Clamp((sum/states.Count)*.7f+worst*.3f,0,100);return new AquariumWelfareResult(score,FishWelfareState.StatusFor(score),
          score<70?"Revisa primero las necesidades con menor puntuación.":"Las necesidades principales están cubiertas.");}
    }
    public static class FishWelfareVisualAdapter
    {
        public static float SpeedMultiplier(FishWelfareStatus status)=>status switch{FishWelfareStatus.Excellent=>1f,FishWelfareStatus.Good=>.96f,FishWelfareStatus.Attention=>.82f,_=>.68f};
        public static float AnimationMultiplier(FishWelfareStatus status)=>status switch{FishWelfareStatus.Excellent=>1f,FishWelfareStatus.Good=>.96f,FishWelfareStatus.Attention=>.86f,_=>.74f};
    }
}
