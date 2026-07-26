using System;
using UnityEngine;

namespace Acuaria.Fish
{
    public enum PopulationValidationStatus { Draft, Debug, Reviewed }
    [Serializable] public sealed class AquariumPopulationEntry
    {
        [SerializeField] FishSpeciesDefinition species;[SerializeField,Min(0)] int quantity=1;
        [SerializeField] int baseSeed;[SerializeField] SwimmingLevel initialZone=SwimmingLevel.Any;
        [SerializeField] string instanceNamePrefix;
        public FishSpeciesDefinition Species=>species;public int Quantity=>quantity;public int BaseSeed=>baseSeed;
        public SwimmingLevel InitialZone=>initialZone;public string InstanceNamePrefix=>instanceNamePrefix;
        public bool IsValid=>species!=null&&species.IsValid&&quantity>0;
        public AquariumPopulationEntry(FishSpeciesDefinition definition,int count,int seed=0,string prefix=null)
        {species=definition;quantity=Mathf.Max(0,count);baseSeed=seed;initialZone=definition!=null?definition.SwimmingLevel:SwimmingLevel.Any;instanceNamePrefix=prefix;}
    }
    [CreateAssetMenu(menuName="Acuaria/Fish/Aquarium Population",fileName="AquariumPopulation")]
    public sealed class AquariumPopulationDefinition:ScriptableObject
    {
        [SerializeField] string populationId,displayName;[SerializeField,TextArea]string description,notes;
        [SerializeField] AquariumPopulationEntry[] entries=Array.Empty<AquariumPopulationEntry>();
        [SerializeField] PopulationValidationStatus validationStatus;
        public string PopulationId=>populationId;public string DisplayName=>displayName;
        public AquariumPopulationEntry[] Entries=>entries;public PopulationValidationStatus ValidationStatus=>validationStatus;
        public int TotalCount{get{var total=0;if(entries!=null)for(var i=0;i<entries.Length;i++)if(entries[i]?.IsValid==true)total+=entries[i].Quantity;return total;}}
        public bool IsValid=>!string.IsNullOrWhiteSpace(populationId)&&entries!=null;
        public void Configure(string id,string label,PopulationValidationStatus status,params AquariumPopulationEntry[] populationEntries)
        {populationId=id;displayName=label;validationStatus=status;entries=populationEntries??Array.Empty<AquariumPopulationEntry>();}
    }
}
