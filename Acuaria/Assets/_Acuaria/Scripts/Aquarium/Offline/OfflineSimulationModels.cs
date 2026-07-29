using System;
using System.Collections.Generic;
using Acuaria.Save;

namespace Acuaria.Offline
{
    public interface IOfflineTimeProvider { DateTime UtcNow { get; } }
    public sealed class SystemOfflineTimeProvider:IOfflineTimeProvider { public DateTime UtcNow=>DateTime.UtcNow; }
    public sealed class FixedOfflineTimeProvider:IOfflineTimeProvider
    {
        public DateTime UtcNow{get;} public FixedOfflineTimeProvider(DateTime value)=>UtcNow=value.Kind==DateTimeKind.Utc?value:value.ToUniversalTime();
    }
    public sealed class OfflineSimulationPolicy
    {
        public bool Enabled,Fish,Water,Cycle,Filter,Maintenance,Welfare,Journal,AllowDeath;
        public double MinimumSeconds,MaximumHours,RollbackToleranceMinutes,LargeJumpHours,StepHours;
        public int MaxJournal,MaxSummary;
        public float MaxHunger,MaxHealthLoss,MinHealth,MaxFilterDirt,MaxWelfareDrop,MaxAmmonia,MaxNitrite,MaxNitrate;
        public static OfflineSimulationPolicy From(OfflineSimulationDefinition d)=>d==null?SafeDefault:new OfflineSimulationPolicy
        {Enabled=d.Enabled,Fish=d.Fish,Water=d.Water,Cycle=d.Cycle,Filter=d.Filter,Maintenance=d.Maintenance,Welfare=d.Welfare,
         Journal=d.Journal,AllowDeath=d.AllowDeath,MinimumSeconds=d.MinimumSeconds,MaximumHours=d.MaximumHours,
         RollbackToleranceMinutes=d.RollbackToleranceMinutes,LargeJumpHours=d.LargeJumpHours,StepHours=d.StepHours,
         MaxJournal=d.MaxJournal,MaxSummary=d.MaxSummary,MaxHunger=d.MaxHunger,MaxHealthLoss=d.MaxHealthLoss,
         MinHealth=d.MinHealth,MaxFilterDirt=d.MaxFilterDirt,MaxWelfareDrop=d.MaxWelfareDrop,
         MaxAmmonia=d.MaxAmmonia,MaxNitrite=d.MaxNitrite,MaxNitrate=d.MaxNitrate};
        public static OfflineSimulationPolicy SafeDefault=>new()
        {Enabled=true,Fish=true,Water=true,Cycle=true,Filter=true,Maintenance=true,Welfare=true,Journal=true,
         MinimumSeconds=300,MaximumHours=48,RollbackToleranceMinutes=5,LargeJumpHours=72,StepHours=1,MaxJournal=4,MaxSummary=5,
         MaxHunger=.75f,MaxHealthLoss=.2f,MinHealth=.55f,MaxFilterDirt=.55f,MaxWelfareDrop=.35f,
         MaxAmmonia=1.5f,MaxNitrite=1.2f,MaxNitrate=35};
    }
    public enum OfflineTimeStatus { Valid,TooShort,RollbackWithinTolerance,ClockRollback,InvalidTimestamp,AlreadyApplied,Disabled }
    public sealed class OfflineTimeValidation
    {
        public OfflineTimeStatus Status; public DateTime StartUtc,EndUtc; public TimeSpan Actual,Effective,Truncated;
        public bool IsValid,WasCapped; public readonly List<string> Warnings=new();
    }
    public sealed class OfflineSimulationContext
    {
        public string SaveId,SimulationVersion,AquariumId,ExecutionKey; public DateTime IntervalStartUtc,IntervalEndUtc;
        public TimeSpan ActualDuration,EffectiveDuration; public OfflineSimulationPolicy Policy;
        public AquariumSaveData Aquarium; public bool IsColdStart,IsResumeFromBackground,IsCapped;
    }
    public enum OfflineEventPriority { Info,Recommendation,Warning }
    [Serializable] public sealed class OfflineEvent
    {
        public string Key,AquariumId,Message; public OfflineEventPriority Priority;
    }
    public sealed class OfflineAquariumResult
    {
        public string AquariumId; public int FishProcessed; public float HungerIncrease,AmmoniaIncrease,NitriteIncrease,NitrateIncrease,FilterDirtIncrease;
        public bool CycleCompleted,WaterExcellent,WelfareExcellent;
        public readonly List<OfflineEvent> Events=new();
    }
    public sealed class OfflineSimulationReport
    {
        public OfflineTimeValidation Time; public string ExecutionKey; public int AquariumsProcessed,FishProcessed;
        public readonly List<OfflineAquariumResult> Aquariums=new(); public readonly List<OfflineEvent> Events=new();
        public bool Applied,AnyExcellentWater,AnyExcellentWelfare; public bool Relevant=>Events.Count>0; public double DurationMilliseconds;
    }
    public sealed class OfflineSimulationResult { public bool Success,WasAlreadyApplied; public string Error; public AcuariaSaveData Data; public OfflineSimulationReport Report; }
    public static class BioloadCalculator
    {
        public static float Calculate(int fishCount,float volumeLiters,float filterEfficiency)
        {
            if(fishCount<=0||!float.IsFinite(volumeLiters)||volumeLiters<=0)return 0;
            return Math.Clamp(fishCount/volumeLiters*(1.25f-Math.Clamp(filterEfficiency,0,1)*.35f),0,10);
        }
    }
}
