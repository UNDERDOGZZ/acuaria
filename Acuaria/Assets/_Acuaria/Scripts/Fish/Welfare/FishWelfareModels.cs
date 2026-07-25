using System;
using System.Collections.Generic;
using Acuaria.Fish.Care;
using Acuaria.Fish.Compatibility;
using Acuaria.Simulation.Water;
using UnityEngine;
namespace Acuaria.Fish.Welfare
{
    public enum FishWelfareStatus { Excellent, Good, Attention, Poor }
    public enum FishWelfareTrend { Improving, Stable, Declining }
    public enum AquariumStockingStatus { Spacious, Appropriate, Crowded, Overcrowded }
    [CreateAssetMenu(menuName="Acuaria/Fish/Welfare Settings")] public sealed class FishWelfareDefinition:ScriptableObject
    {
        [SerializeField] string settingsId="starter-fish-welfare";[SerializeField] Vector4 primaryWeights=new(1.2f,1.1f,1.3f,1.5f);
        [SerializeField] Vector3 secondaryWeights=new(.9f,1.1f,.7f);[SerializeField] float improvementPerHour=12,deteriorationPerHour=20,toleranceHours=.05f;
        [SerializeField] Vector3 thresholds=new(40,70,90);[SerializeField] float messageCooldown=5;
        public string SettingsId=>settingsId;public float TemperatureWeight=>primaryWeights.x;public float VolumeWeight=>primaryWeights.y;
        public float SocialWeight=>primaryWeights.z;public float WaterWeight=>primaryWeights.w;public float FeedingWeight=>secondaryWeights.x;
        public float CompatibilityWeight=>secondaryWeights.y;public float ZoneWeight=>secondaryWeights.z;
        public float ImprovementPerHour=>improvementPerHour;public float DeteriorationPerHour=>deteriorationPerHour;public float ToleranceHours=>toleranceHours;
        public Vector3 Thresholds=>thresholds;public bool IsValid=>!string.IsNullOrWhiteSpace(settingsId)&&WeightSum>0&&improvementPerHour>0&&deteriorationPerHour>0&&thresholds.x<=thresholds.y&&thresholds.y<=thresholds.z;
        public float WeightSum=>primaryWeights.x+primaryWeights.y+primaryWeights.z+primaryWeights.w+secondaryWeights.x+secondaryWeights.y+secondaryWeights.z;
        public void Configure(string id,Vector4 primary,Vector3 secondary,Vector3 timing,Vector3 limits,float cooldown)
        {settingsId=id;primaryWeights=Positive(primary);secondaryWeights=Positive(secondary);improvementPerHour=Mathf.Max(.01f,Safe(timing.x));
         deteriorationPerHour=Mathf.Max(.01f,Safe(timing.y));toleranceHours=Safe(timing.z);thresholds=limits;messageCooldown=Safe(cooldown);}
        static Vector4 Positive(Vector4 v)=>new(Safe(v.x),Safe(v.y),Safe(v.z),Safe(v.w));static Vector3 Positive(Vector3 v)=>new(Safe(v.x),Safe(v.y),Safe(v.z));
        static float Safe(float v)=>float.IsFinite(v)?Mathf.Max(0,v):0;
    }
    [Serializable] public sealed class FishWelfareState
    {
        public string FishInstanceId{get;private set;}public float CurrentScore{get;private set;}public float TargetScore{get;private set;}
        public FishWelfareTrend Trend{get;private set;}public FishWelfareStatus Status{get;private set;}
        public float HoursUnderStress{get;private set;}public float StableHours{get;private set;}public bool IsInitialized{get;private set;}
        public IReadOnlyList<string> ActiveIssues=>issues;readonly List<string> issues=new();
        public void Initialize(string id,float score=85){if(string.IsNullOrWhiteSpace(id))throw new ArgumentException();FishInstanceId=id;CurrentScore=Mathf.Clamp(score,0,100);TargetScore=CurrentScore;Trend=FishWelfareTrend.Stable;Status=StatusFor(CurrentScore);IsInitialized=true;}
        public void Apply(float score,float target,float hours,IReadOnlyList<string> activeIssues)
        {CurrentScore=Mathf.Clamp(Safe(score),0,100);TargetScore=Mathf.Clamp(Safe(target),0,100);Trend=CurrentScore<TargetScore-.01f?FishWelfareTrend.Improving:CurrentScore>TargetScore+.01f?FishWelfareTrend.Declining:FishWelfareTrend.Stable;
         Status=StatusFor(CurrentScore);if(TargetScore<70){HoursUnderStress+=Safe(hours);StableHours=0;}else{StableHours+=Safe(hours);HoursUnderStress=0;}
         issues.Clear();if(activeIssues!=null)for(var i=0;i<activeIssues.Count;i++)issues.Add(activeIssues[i]);}
        public static FishWelfareStatus StatusFor(float score)=>score>=90?FishWelfareStatus.Excellent:score>=70?FishWelfareStatus.Good:score>=40?FishWelfareStatus.Attention:FishWelfareStatus.Poor;
        static float Safe(float v)=>float.IsFinite(v)?Mathf.Max(0,v):0;
    }
    public readonly struct AquariumStockingResult
    {public readonly float Demand,Occupancy;public readonly AquariumStockingStatus Status;public AquariumStockingResult(float d,float o,AquariumStockingStatus s){Demand=d;Occupancy=o;Status=s;}}
    public sealed class AquariumStockingModel
    {
        public AquariumStockingResult Evaluate(IReadOnlyList<FishSpeciesDefinition> species,float volume)
        {var demand=0f;if(species!=null)for(var i=0;i<species.Count;i++){var care=species[i]?.Care;if(care==null)continue;
          var activity=care.Activity==FishActivityLevel.High?1.35f:care.Activity==FishActivityLevel.Low?.8f:1f;demand+=care.MinimumIndividualVolume*activity;}
         var occupancy=volume<=0?1f:demand/volume;var status=species==null||species.Count==0?AquariumStockingStatus.Spacious:
          occupancy<=.55f?AquariumStockingStatus.Spacious:occupancy<=1?AquariumStockingStatus.Appropriate:occupancy<=1.35f?AquariumStockingStatus.Crowded:AquariumStockingStatus.Overcrowded;
         return new AquariumStockingResult(demand,Mathf.Max(0,occupancy),status);}
    }
    public readonly struct FishWelfareEvaluationResult
    {
        public readonly bool IsValid;public readonly float OverallScore,TemperatureScore,VolumeScore,SocialScore,WaterQualityScore,FeedingScore,CompatibilityScore,SwimmingZoneScore;
        public readonly FishWelfareStatus Status;public readonly IReadOnlyList<string> ActiveIssues,Recommendations;
        public FishWelfareEvaluationResult(bool valid,float overall,float temp,float volume,float social,float water,float feed,float compatibility,float zone,List<string> issues,List<string> recommendations)
        {IsValid=valid;OverallScore=overall;TemperatureScore=temp;VolumeScore=volume;SocialScore=social;WaterQualityScore=water;FeedingScore=feed;CompatibilityScore=compatibility;SwimmingZoneScore=zone;Status=FishWelfareState.StatusFor(overall);ActiveIssues=issues;Recommendations=recommendations;}
    }
    public readonly struct FishWelfareContext
    {
        public readonly float Temperature,Volume,Satiety;public readonly int SameSpeciesCount;public readonly WaterQualityStatus WaterQuality;
        public readonly FishCompatibilityStatus Compatibility;public readonly AquariumHabitatProfile Habitat;public readonly AquariumStockingStatus Stocking;
        public FishWelfareContext(float temp,float volume,float satiety,int same,WaterQualityStatus water,FishCompatibilityStatus compatibility,AquariumHabitatProfile habitat,AquariumStockingStatus stocking)
        {Temperature=temp;Volume=volume;Satiety=satiety;SameSpeciesCount=same;WaterQuality=water;Compatibility=compatibility;Habitat=habitat;Stocking=stocking;}
    }
}
