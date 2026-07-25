using System.Collections.Generic;
using System.Text;
using Acuaria.Fish;
using Acuaria.Fish.Compatibility;
using Acuaria.Fish.Welfare;
namespace Acuaria.UI.FishWelfare
{
    public sealed class FishWelfareViewModel
    {
        public string CompactText{get;}public string DetailsText{get;}public string CompatibilityText{get;}
        public FishWelfareViewModel(AquariumWelfareResult aquarium,IReadOnlyList<FishMovement2D> fish,
            IReadOnlyList<FishWelfareState> states,AquariumCompatibilityReport compatibility)
        {
            CompactText=$"Bienestar: {Label(aquarium.Status)} {aquarium.Score:0}/100";
            var builder=new StringBuilder();builder.AppendLine($"Bienestar general: {Label(aquarium.Status)} ({aquarium.Score:0}/100)");
            builder.AppendLine(aquarium.Recommendation).AppendLine();
            for(var i=0;i<states.Count&&i<fish.Count;i++){var species=fish[i]?.Species;var state=states[i];if(species==null||state==null)continue;
                builder.AppendLine($"{species.DisplayName}: {Label(state.Status)} {state.CurrentScore:0}/100 {Trend(state.Trend)}");
                builder.AppendLine($"  Temperatura {species.Care.TemperatureRange.x:0}–{species.Care.TemperatureRange.y:0} °C · Volumen de grupo {species.Care.MinimumGroupVolume:0} L");
                builder.AppendLine($"  Grupo recomendado {species.Social.RecommendedMinimum} · Zona {species.Care.SwimmingZone}");
                if(state.ActiveIssues.Count>0)builder.AppendLine($"  Prioridad: {state.ActiveIssues[0]}");}
            DetailsText=builder.ToString();CompatibilityText=compatibility==null?"Compatibilidad no disponible":
                $"Compatibilidad general: {compatibility.OverallStatus}\n"+string.Join("\n",compatibility.PairSummaries);
        }
        static string Label(FishWelfareStatus s)=>s switch{FishWelfareStatus.Excellent=>"Excelente",FishWelfareStatus.Good=>"Bien",FishWelfareStatus.Attention=>"Atención",_=>"Bajo"};
        static string Trend(FishWelfareTrend t)=>t switch{FishWelfareTrend.Improving=>"↑",FishWelfareTrend.Declining=>"↓",_=>"→"};
    }
}
