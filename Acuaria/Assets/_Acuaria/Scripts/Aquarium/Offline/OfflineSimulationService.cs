using System;
using System.Collections.Generic;
using System.Diagnostics;
using Acuaria.Save;

namespace Acuaria.Offline
{
    public sealed class OfflineTimeValidator
    {
        public OfflineTimeValidation Validate(AcuariaSaveData data,DateTime now,OfflineSimulationPolicy policy)
        {
            var result=new OfflineTimeValidation{EndUtc=now};
            if(policy?.Enabled!=true){result.Status=OfflineTimeStatus.Disabled;return result;}
            if(data==null||now.Kind!=DateTimeKind.Utc){result.Status=OfflineTimeStatus.InvalidTimestamp;return result;}
            var candidates=new[]{data.LastSimulationAtUtc,data.UpdatedAtUtc,data.LastSessionEndedAtUtc,data.LastApplicationPauseAtUtc};
            var found=false;var start=DateTime.MinValue;
            foreach(var value in candidates)if(ParseUtc(value,out var parsed)&&(!found||parsed>start)){start=parsed;found=true;}
            if(!found){result.Status=OfflineTimeStatus.InvalidTimestamp;return result;}
            result.StartUtc=start;result.Actual=now-start;
            if(result.Actual.TotalSeconds<0)
            {
                result.Status=Math.Abs(result.Actual.TotalMinutes)<=policy.RollbackToleranceMinutes
                    ?OfflineTimeStatus.RollbackWithinTolerance:OfflineTimeStatus.ClockRollback;
                if(result.Status==OfflineTimeStatus.ClockRollback)result.Warnings.Add("El reloj retrocedió; no se aplicó progreso offline.");
                return result;
            }
            if(result.Actual.TotalSeconds<policy.MinimumSeconds){result.Status=OfflineTimeStatus.TooShort;return result;}
            var maximum=TimeSpan.FromHours(policy.MaximumHours);result.Effective=result.Actual>maximum?maximum:result.Actual;
            result.Truncated=result.Actual-result.Effective;result.WasCapped=result.Truncated>TimeSpan.Zero;
            if(result.WasCapped)result.Warnings.Add("El intervalo fue limitado por la política offline.");
            if(result.Actual.TotalHours>policy.LargeJumpHours)result.Warnings.Add("Se detectó un salto grande del reloj.");
            result.Status=OfflineTimeStatus.Valid;result.IsValid=true;return result;
        }
        public static bool ParseUtc(string value,out DateTime parsed)=>
            DateTime.TryParse(value,null,System.Globalization.DateTimeStyles.RoundtripKind,out parsed)&&parsed.Kind==DateTimeKind.Utc;
    }

    public sealed class OfflineEventAggregator
    {
        public List<OfflineEvent> Aggregate(IEnumerable<OfflineEvent> source,int maximum)
        {
            var byKey=new Dictionary<string,OfflineEvent>(StringComparer.Ordinal);
            if(source!=null)foreach(var item in source)
            {
                if(item==null||string.IsNullOrWhiteSpace(item.Key))continue;
                var key=$"{item.AquariumId}:{item.Key}";
                if(!byKey.TryGetValue(key,out var existing)||item.Priority>existing.Priority)byKey[key]=item;
            }
            var result=new List<OfflineEvent>(byKey.Values);
            result.Sort((a,b)=>{var priority=b.Priority.CompareTo(a.Priority);return priority!=0?priority:string.CompareOrdinal(a.Key,b.Key);});
            if(result.Count>Math.Max(0,maximum))result.RemoveRange(maximum,result.Count-maximum);
            return result;
        }
    }
    public sealed class OfflineJournalGenerator
    {
        public void Apply(AcuariaSaveData data,IReadOnlyList<OfflineEvent> events,DateTime timestamp,int maximum)
        {
            if(data?.Aquariums==null||events==null)return;var count=0;
            foreach(var item in events)
            {
                if(count>=maximum)break;
                var aquarium=data.Aquariums.Find(value=>value.AquariumInstanceId==item.AquariumId);
                if(aquarium==null)continue;aquarium.Journal??=new AquariumJournalSaveData{AquariumInstanceId=aquarium.AquariumInstanceId};
                aquarium.Journal.Entries??=new List<JournalEntrySaveData>();
                aquarium.Journal.Entries.Add(new JournalEntrySaveData
                {EntryId=$"offline-{data.OfflineSimulationSequence}-{count}",EntryType="Offline",TimestampUtc=timestamp.ToString("O"),
                 AquariumInstanceId=aquarium.AquariumInstanceId,Content=item.Message,IsImportant=item.Priority==OfflineEventPriority.Warning});
                count++;
            }
        }
    }

    public sealed class OfflineSimulationPipeline
    {
        public OfflineAquariumResult Simulate(OfflineSimulationContext context)
        {
            var aquarium=context.Aquarium;var policy=context.Policy;var hours=(float)context.EffectiveDuration.TotalHours;
            var result=new OfflineAquariumResult{AquariumId=aquarium.AquariumInstanceId};
            aquarium.Fish??=new List<FishSaveData>();aquarium.WaterState??=new WaterStateSaveData();
            aquarium.FilterState??=new FilterStateSaveData();aquarium.NitrogenCycleState??=new NitrogenCycleSaveData();
            var filterEfficiency=Finite01(aquarium.FilterState.Efficiency,1);
            var bioload=BioloadCalculator.Calculate(aquarium.Fish.Count,Math.Max(.1f,aquarium.RuntimeState?.VolumeLiters??50),filterEfficiency);
            if(policy.Fish)foreach(var fish in aquarium.Fish)
            {
                if(fish==null)continue;result.FishProcessed++;
                var increase=Math.Min(policy.MaxHunger,hours*.018f);
                var previousHunger=Finite01(fish.Hunger,1-Finite01(fish.Satiety,.45f));
                fish.Hunger=Math.Clamp(previousHunger+increase,0,1);fish.Satiety=Math.Clamp(1-fish.Hunger,0,1);
                var stressIncrease=Math.Max(0,fish.Hunger-.65f)*.2f*Math.Min(1,hours/24f);
                fish.Stress=Math.Clamp(Finite01(fish.Stress,0)+stressIncrease,0,1);
                var healthLoss=Math.Min(policy.MaxHealthLoss,Math.Max(0,fish.Hunger-.8f)*.08f*Math.Min(1,hours/24f));
                fish.Health=Math.Max(policy.MinHealth,Finite01(fish.Health,1)-healthLoss);fish.IsAlive=true;
                fish.Welfare=Math.Clamp(1-(fish.Hunger*.35f+fish.Stress*.3f+(1-fish.Health)*.35f),0,1);
                result.HungerIncrease+=fish.Hunger-previousHunger;
            }
            if(policy.Water&&aquarium.Fish.Count>0)
            {
                var rawAmmonia=bioload*hours*.035f;var processed=rawAmmonia*Math.Clamp(aquarium.WaterState.AmmoniaBacteria,0,1)*filterEfficiency*.55f;
                var ammoniaDelta=Math.Min(policy.MaxAmmonia,Math.Max(0,rawAmmonia-processed));
                var nitriteProduced=processed;var nitriteProcessed=nitriteProduced*Math.Clamp(aquarium.WaterState.NitriteBacteria,0,1)*filterEfficiency*.5f;
                var nitriteDelta=Math.Min(policy.MaxNitrite,Math.Max(0,nitriteProduced-nitriteProcessed));
                var nitrateDelta=Math.Min(policy.MaxNitrate,Math.Max(0,nitriteProcessed));
                aquarium.WaterState.AmmoniaPpm=Math.Max(0,aquarium.WaterState.AmmoniaPpm+ammoniaDelta);
                aquarium.WaterState.NitritePpm=Math.Max(0,aquarium.WaterState.NitritePpm+nitriteDelta);
                aquarium.WaterState.NitratePpm=Math.Max(0,aquarium.WaterState.NitratePpm+nitrateDelta);
                aquarium.WaterState.OrganicWaste=Math.Max(0,aquarium.WaterState.OrganicWaste+bioload*hours);
                aquarium.WaterState.TotalSimulatedSeconds+=context.EffectiveDuration.TotalSeconds;
                aquarium.WaterState.LastUpdatedAtUtc=context.IntervalEndUtc.ToString("O");
                result.AmmoniaIncrease=ammoniaDelta;result.NitriteIncrease=nitriteDelta;result.NitrateIncrease=nitrateDelta;
            }
            if(policy.Cycle)
            {
                var wasEstablished=aquarium.NitrogenCycleState.IsEstablished;
                aquarium.NitrogenCycleState.TotalElapsedSimulationSeconds+=context.EffectiveDuration.TotalSeconds;
                aquarium.NitrogenCycleState.Progress=Math.Clamp(aquarium.NitrogenCycleState.Progress+hours/720f,0,1);
                aquarium.NitrogenCycleState.IsEstablished=aquarium.NitrogenCycleState.Progress>=1;
                result.CycleCompleted=!wasEstablished&&aquarium.NitrogenCycleState.IsEstablished;
                aquarium.NitrogenCycleState.BacteriaLevel=Math.Clamp((aquarium.WaterState.AmmoniaBacteria+aquarium.WaterState.NitriteBacteria)*.5f,0,1);
                aquarium.NitrogenCycleState.LastTickAtUtc=context.IntervalEndUtc.ToString("O");
            }
            if(policy.Filter&&aquarium.Fish.Count>0)
            {
                var increase=Math.Min(policy.MaxFilterDirt,bioload*hours*.012f);
                aquarium.FilterState.DirtLevel=Math.Clamp(aquarium.FilterState.DirtLevel+increase,0,1);
                aquarium.FilterState.Efficiency=Math.Clamp(filterEfficiency*(1-increase*.45f),.2f,1);
                aquarium.FilterState.HoursSinceMaintenance+=hours;aquarium.FilterState.IsRunning=true;
                aquarium.FilterState.MaintenanceRecommended=aquarium.FilterState.DirtLevel>=.65f;result.FilterDirtIncrease=increase;
            }
            if(policy.Welfare)
            {
                aquarium.WelfareState??=new WelfareSaveData();var average=1f;
                if(aquarium.Fish.Count>0){average=0;foreach(var fish in aquarium.Fish)average+=fish.Welfare;average/=aquarium.Fish.Count;}
                var waterPenalty=Math.Clamp(aquarium.WaterState.AmmoniaPpm*.35f+aquarium.WaterState.NitritePpm*.35f,0,.7f);
                aquarium.WelfareState.OverallScore=Math.Clamp((average-waterPenalty)*100,0,100);
                aquarium.WelfareState.LastEvaluatedAtUtc=context.IntervalEndUtc.ToString("O");
            }
            result.WaterExcellent=aquarium.WaterState.AmmoniaPpm<.1f&&aquarium.WaterState.NitritePpm<.1f&&aquarium.WaterState.NitratePpm<20f;
            result.WelfareExcellent=(aquarium.WelfareState?.OverallScore??0)>=80f;
            AddEvents(aquarium,result);
            return result;
        }
        static void AddEvents(AquariumSaveData aquarium,OfflineAquariumResult result)
        {
            if(result.FishProcessed>0&&result.HungerIncrease/result.FishProcessed>.15f)result.Events.Add(Event("feeding",aquarium,"Conviene revisar la alimentación de los peces.",OfflineEventPriority.Recommendation));
            if(result.AmmoniaIncrease>.15f||result.NitriteIncrease>.1f)result.Events.Add(Event("water",aquarium,"La química cambió durante tu ausencia; revisa el agua.",OfflineEventPriority.Warning));
            if(aquarium.FilterState.MaintenanceRecommended)result.Events.Add(Event("filter",aquarium,"El filtro necesita mantenimiento.",OfflineEventPriority.Warning));
            if(result.CycleCompleted)result.Events.Add(Event("cycle",aquarium,"El ciclo biológico está establecido.",OfflineEventPriority.Info));
        }
        static OfflineEvent Event(string key,AquariumSaveData aquarium,string message,OfflineEventPriority priority)=>
            new(){Key=key,AquariumId=aquarium.AquariumInstanceId,Message=$"{aquarium.DisplayName}: {message}",Priority=priority};
        static float Finite01(float value,float fallback)=>float.IsFinite(value)?Math.Clamp(value,0,1):fallback;
    }

    public sealed class OfflineSimulationService
    {
        readonly IOfflineTimeProvider time;readonly OfflineTimeValidator validator;readonly OfflineSimulationPipeline pipeline;
        readonly OfflineEventAggregator aggregator=new();readonly OfflineJournalGenerator journal=new();
        public OfflineSimulationService(IOfflineTimeProvider provider,OfflineTimeValidator timeValidator=null,OfflineSimulationPipeline simulationPipeline=null)
        {time=provider??throw new ArgumentNullException(nameof(provider));validator=timeValidator??new OfflineTimeValidator();pipeline=simulationPipeline??new OfflineSimulationPipeline();}
        public OfflineSimulationResult Simulate(AcuariaSaveData data,OfflineSimulationPolicy policy,bool coldStart)
        {
            var report=new OfflineSimulationReport();var watch=Stopwatch.StartNew();var now=time.UtcNow;
            if(data!=null&&OfflineTimeValidator.ParseUtc(data.LastAppliedOfflineIntervalEndUtc,out var appliedEnd)&&now==appliedEnd)
            {
                report.Time=new OfflineTimeValidation{Status=OfflineTimeStatus.AlreadyApplied,StartUtc=appliedEnd,EndUtc=now};
                return new OfflineSimulationResult{Data=data,Report=report,Success=true,WasAlreadyApplied=true};
            }
            var validation=validator.Validate(data,now,policy);report.Time=validation;
            var result=new OfflineSimulationResult{Data=data,Report=report};
            if(!validation.IsValid){result.Success=validation.Status is OfflineTimeStatus.TooShort or OfflineTimeStatus.RollbackWithinTolerance or OfflineTimeStatus.Disabled;return result;}
            var key=$"{data.SaveId}|{validation.StartUtc:O}|{validation.EndUtc:O}|1";
            if(string.Equals(key,data.LastOfflineExecutionKey,StringComparison.Ordinal)){result.Success=true;result.WasAlreadyApplied=true;return result;}
            report.ExecutionKey=key;
            foreach(var aquarium in data.Aquariums??new List<AquariumSaveData>())
            {
                if(aquarium==null||!aquarium.IsInitialized)continue;
                var context=new OfflineSimulationContext{SaveId=data.SaveId,SimulationVersion="1",ExecutionKey=key,
                    IntervalStartUtc=validation.StartUtc,IntervalEndUtc=validation.EndUtc,ActualDuration=validation.Actual,
                    EffectiveDuration=validation.Effective,Policy=policy,Aquarium=aquarium,AquariumId=aquarium.AquariumInstanceId,
                    IsColdStart=coldStart,IsResumeFromBackground=!coldStart,IsCapped=validation.WasCapped};
                var aquariumResult=pipeline.Simulate(context);report.Aquariums.Add(aquariumResult);
                report.AquariumsProcessed++;report.FishProcessed+=aquariumResult.FishProcessed;report.Events.AddRange(aquariumResult.Events);
                report.AnyExcellentWater|=aquariumResult.WaterExcellent;report.AnyExcellentWelfare|=aquariumResult.WelfareExcellent;
            }
            var consolidated=aggregator.Aggregate(report.Events,policy.MaxJournal);report.Events.Clear();report.Events.AddRange(consolidated);
            data.OfflineSimulationSequence++;data.LastAppliedOfflineIntervalStartUtc=validation.StartUtc.ToString("O");
            data.LastAppliedOfflineIntervalEndUtc=validation.EndUtc.ToString("O");data.LastSimulationAtUtc=validation.EndUtc.ToString("O");
            data.LastOfflineExecutionKey=key;data.SimulationVersion="1";data.GlobalStatistics??=new StatisticsSaveData();
            data.GlobalStatistics.OfflineSessions++;data.GlobalStatistics.TotalOfflineTimeSeconds+=validation.Effective.TotalSeconds;
            if(validation.WasCapped)data.GlobalStatistics.CappedOfflineSessions++;
            if(policy.Journal)journal.Apply(data,consolidated,validation.EndUtc,policy.MaxJournal);
            report.Applied=true;watch.Stop();report.DurationMilliseconds=watch.Elapsed.TotalMilliseconds;result.Success=true;return result;
        }
    }
}
