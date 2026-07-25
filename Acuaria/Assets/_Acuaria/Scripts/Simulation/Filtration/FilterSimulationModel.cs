using UnityEngine;
namespace Acuaria.Simulation.Filtration
{
    public sealed class FilterSimulationModel
    {
        public void Step(FilterRuntimeState state,FilterDefinition definition,float volume,float waste,float hours)
        {
            if(state==null||definition==null||!state.IsInitialized||hours<=0f||!float.IsFinite(hours))return;
            var compatibility=volume<definition.RecommendedVolume.x||volume>definition.RecommendedVolume.y?0.8f:1f;
            var dirt=Mathf.Clamp01(state.DirtLevel+definition.DirtAccumulationPerHour*hours*(1f+Mathf.Max(0f,waste)*0.02f));
            var efficiency=state.IsActive?definition.BaseEfficiency*compatibility*(1f-dirt*definition.MaximumDirtPenalty):0f;
            var recommended=dirt>=0.65f||state.HoursSinceMaintenance+hours>=definition.RecommendedMaintenanceHours;
            var status=!state.IsActive?FilterOperatingStatus.Off:recommended?FilterOperatingStatus.MaintenanceRecommended:
                efficiency<definition.BaseEfficiency*0.65f?FilterOperatingStatus.ReducedEfficiency:FilterOperatingStatus.Running;
            state.Apply(dirt,efficiency,definition.AdditionalBiologicalCapacity*efficiency,state.HoursSinceMaintenance+hours,status,recommended);
        }
    }
    public enum FilterMaintenanceType { GentleRinse, DeepClean }
    public readonly struct FilterMaintenanceResult
    { public readonly bool IsValid;public readonly float Dirt,BiologicalCapacity,BacteriaRetention;
      public FilterMaintenanceResult(bool valid,float dirt,float capacity,float retention){IsValid=valid;Dirt=dirt;BiologicalCapacity=capacity;BacteriaRetention=retention;} }
    public sealed class FilterMaintenanceModel
    {
        public FilterMaintenanceResult Calculate(FilterRuntimeState state,FilterMaintenanceType type)
        {
            if(state==null||!state.IsInitialized)return default;
            var deep=type==FilterMaintenanceType.DeepClean;var retention=deep?0.45f:0.9f;
            return new FilterMaintenanceResult(true,state.DirtLevel*(deep?0.15f:0.45f),state.BiologicalCapacity*retention,retention);
        }
        public bool Apply(FilterRuntimeState state,FilterDefinition definition,FilterMaintenanceType type)
        {
            var result=Calculate(state,type);if(!result.IsValid||definition==null)return false;
            var efficiency=definition.BaseEfficiency*(1f-result.Dirt*definition.MaximumDirtPenalty);
            state.Apply(result.Dirt,efficiency,result.BiologicalCapacity,0f,FilterOperatingStatus.Running,false);state.RecordMaintenance();return true;
        }
    }
}
