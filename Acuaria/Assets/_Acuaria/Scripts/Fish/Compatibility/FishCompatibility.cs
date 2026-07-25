using System;
using System.Collections.Generic;
using Acuaria.Fish.Care;
using UnityEngine;
namespace Acuaria.Fish.Compatibility
{
    public enum FishCompatibilityStatus { Compatible, Caution, Incompatible }
    [Serializable] public sealed class FishCompatibilityProfile
    {
        [SerializeField] Vector2 acceptableCompanionSize=new(1,20);[SerializeField] bool peaceful=true,semiTerritorial=true,territorial;
        [SerializeField] bool slow=true,active=true;[SerializeField] bool sameZoneConflict;[SerializeField] bool conceptualPredationRisk;
        [SerializeField] string[] compatibleSpecies=Array.Empty<string>(),incompatibleSpecies=Array.Empty<string>();
        public Vector2 SizeRange=>acceptableCompanionSize;public bool SameZoneConflict=>sameZoneConflict;
        public string[] CompatibleSpecies=>compatibleSpecies;public string[] IncompatibleSpecies=>incompatibleSpecies;
        public void Configure(Vector2 size,bool peace,bool semi,bool territory,bool slowFish,bool activeFish,bool zoneConflict,bool predation,
            string[] compatible=null,string[] incompatible=null)
        {acceptableCompanionSize=new Vector2(Mathf.Max(0,size.x),Mathf.Max(size.x,size.y));peaceful=peace;semiTerritorial=semi;territorial=territory;
         slow=slowFish;active=activeFish;sameZoneConflict=zoneConflict;conceptualPredationRisk=predation;
         compatibleSpecies=compatible??Array.Empty<string>();incompatibleSpecies=incompatible??Array.Empty<string>();}
        public bool Accepts(FishTerritoriality value)=>value switch{FishTerritoriality.Peaceful=>peaceful,FishTerritoriality.SemiTerritorial=>semiTerritorial,_=>territorial};
    }
    public readonly struct FishCompatibilityResult
    {public readonly FishCompatibilityStatus Status;public readonly string Issue;public FishCompatibilityResult(FishCompatibilityStatus s,string i){Status=s;Issue=i;}}
    public sealed class FishCompatibilityEvaluator
    {
        public FishCompatibilityResult Evaluate(FishSpeciesDefinition a,FishSpeciesDefinition b,float volume)
        {
            if(a==null||b==null||a.Care==null||b.Care==null)return new FishCompatibilityResult(FishCompatibilityStatus.Incompatible,"Datos incompletos");
            if(Contains(a.Compatibility.IncompatibleSpecies,b.SpeciesId)||Contains(b.Compatibility.IncompatibleSpecies,a.SpeciesId))
                return new FishCompatibilityResult(FishCompatibilityStatus.Incompatible,"Override de incompatibilidad");
            if(Contains(a.Compatibility.CompatibleSpecies,b.SpeciesId)&&Contains(b.Compatibility.CompatibleSpecies,a.SpeciesId))
                return new FishCompatibilityResult(FishCompatibilityStatus.Compatible,"Compatibilidad específica");
            var min=Mathf.Max(a.Care.TemperatureRange.x,b.Care.TemperatureRange.x);var max=Mathf.Min(a.Care.TemperatureRange.y,b.Care.TemperatureRange.y);
            if(min>max)return new FishCompatibilityResult(FishCompatibilityStatus.Incompatible,"Rangos de temperatura no coinciden");
            if(!a.Compatibility.Accepts(b.Social.Territoriality)||!b.Compatibility.Accepts(a.Social.Territoriality))
                return new FishCompatibilityResult(FishCompatibilityStatus.Incompatible,"Territorialidad incompatible");
            if(a.Social.Territoriality==FishTerritoriality.Territorial&&b.Social.Territoriality==FishTerritoriality.Territorial&&volume<100)
                return new FishCompatibilityResult(FishCompatibilityStatus.Incompatible,"Poco espacio para dos especies territoriales");
            if(a.Care.Activity!=b.Care.Activity||a.SwimmingLevel==b.SwimmingLevel&&(a.Compatibility.SameZoneConflict||b.Compatibility.SameZoneConflict))
                return new FishCompatibilityResult(FishCompatibilityStatus.Caution,"Actividad o zona compartida requiere observación");
            return new FishCompatibilityResult(FishCompatibilityStatus.Compatible,"Necesidades compatibles");
        }
        static bool Contains(string[] values,string id)=>values!=null&&Array.IndexOf(values,id)>=0;
    }
    public sealed class AquariumCompatibilityReport
    {
        public FishCompatibilityStatus OverallStatus{get;}public IReadOnlyList<string> PairSummaries{get;}
        public AquariumCompatibilityReport(IReadOnlyList<FishSpeciesDefinition> species,float volume)
        {var list=new List<string>();var worst=FishCompatibilityStatus.Compatible;var evaluator=new FishCompatibilityEvaluator();
         if(species!=null)for(var i=0;i<species.Count;i++)for(var j=i+1;j<species.Count;j++){if(species[i]==null||species[j]==null)continue;
          var result=evaluator.Evaluate(species[i],species[j],volume);if(result.Status>worst)worst=result.Status;
          list.Add($"{species[i].DisplayName} + {species[j].DisplayName}: {result.Status} — {result.Issue}");}
         OverallStatus=worst;PairSummaries=list;}
    }
}
