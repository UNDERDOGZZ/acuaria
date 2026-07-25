using System;
using UnityEngine;
namespace Acuaria.Fish.Care
{
    public enum FishActivityLevel { Low, Moderate, High }
    public enum FishWaterSensitivity { Hardy, Moderate, Sensitive }
    public enum FishDietType { Herbivore, Omnivore, Carnivore }
    public enum FishSocialType { Solitary, Pair, Group, Schooling }
    public enum FishTerritoriality { Peaceful, SemiTerritorial, Territorial }
    [Serializable] public sealed class FishCareRequirements
    {
        [SerializeField] Vector2 temperatureRange=new(24,26);[SerializeField] float minimumIndividualVolume=20;
        [SerializeField] float minimumGroupVolume=40;[SerializeField] float adultSizeCm=5;
        [SerializeField] FishActivityLevel activity=FishActivityLevel.Moderate;[SerializeField] SwimmingLevel swimmingZone=SwimmingLevel.Any;
        [SerializeField] FishWaterSensitivity waterSensitivity=FishWaterSensitivity.Moderate;
        [SerializeField] bool needsHidingPlaces;[SerializeField] bool needsPlants;
        [SerializeField] Vector3 chemicalSensitivity=Vector3.one;[SerializeField] float feedingFrequencyPerDay=1;
        [SerializeField] FishDietType diet=FishDietType.Omnivore;
        public Vector2 TemperatureRange=>temperatureRange;public float MinimumIndividualVolume=>minimumIndividualVolume;
        public float MinimumGroupVolume=>minimumGroupVolume;public float AdultSizeCm=>adultSizeCm;
        public FishActivityLevel Activity=>activity;public SwimmingLevel SwimmingZone=>swimmingZone;
        public FishWaterSensitivity WaterSensitivity=>waterSensitivity;public bool NeedsHidingPlaces=>needsHidingPlaces;
        public bool NeedsPlants=>needsPlants;public Vector3 ChemicalSensitivity=>chemicalSensitivity;
        public float FeedingFrequencyPerDay=>feedingFrequencyPerDay;public FishDietType Diet=>diet;
        public bool IsValid=>Finite(temperatureRange.x)&&Finite(temperatureRange.y)&&temperatureRange.y>=temperatureRange.x&&
            minimumIndividualVolume>0&&minimumGroupVolume>=minimumIndividualVolume&&adultSizeCm>0&&feedingFrequencyPerDay>=0;
        public void Configure(Vector2 temperature,float individual,float group,float size,FishActivityLevel activityLevel,
            SwimmingLevel zone,FishWaterSensitivity sensitivity,bool hides,bool plants,Vector3 chemicals,float feeding,FishDietType dietType)
        {temperatureRange=new Vector2(Safe(temperature.x),Safe(temperature.y));minimumIndividualVolume=Mathf.Max(.1f,Safe(individual));
         minimumGroupVolume=Mathf.Max(minimumIndividualVolume,Safe(group));adultSizeCm=Mathf.Max(.1f,Safe(size));activity=activityLevel;
         swimmingZone=zone;waterSensitivity=sensitivity;needsHidingPlaces=hides;needsPlants=plants;
         chemicalSensitivity=new Vector3(Safe(chemicals.x),Safe(chemicals.y),Safe(chemicals.z));feedingFrequencyPerDay=Safe(feeding);diet=dietType;}
        static bool Finite(float v)=>float.IsFinite(v);static float Safe(float v)=>Finite(v)?Mathf.Max(0,v):0;
    }
    [Serializable] public sealed class FishSocialRequirements
    {
        [SerializeField] FishSocialType socialType=FishSocialType.Group;[SerializeField] int recommendedMinimum=2;
        [SerializeField] int provisionalMaximum=8;[SerializeField] bool toleratesSolitary;[SerializeField] FishTerritoriality territoriality;
        [SerializeField] bool coexistsWithSameSpecies=true;[SerializeField] bool needsSchool;[SerializeField] bool needsPair;
        [SerializeField] bool toleratesActiveSpecies=true;[SerializeField] bool toleratesTerritorialSpecies;
        public FishSocialType SocialType=>socialType;public int RecommendedMinimum=>recommendedMinimum;public int ProvisionalMaximum=>provisionalMaximum;
        public FishTerritoriality Territoriality=>territoriality;public bool ToleratesSolitary=>toleratesSolitary;
        public bool CoexistsWithSameSpecies=>coexistsWithSameSpecies;public bool ToleratesActiveSpecies=>toleratesActiveSpecies;
        public bool ToleratesTerritorialSpecies=>toleratesTerritorialSpecies;
        public bool IsValid=>recommendedMinimum>=0&&provisionalMaximum>=recommendedMinimum;
        public void Configure(FishSocialType type,int minimum,int maximum,bool solitary,FishTerritoriality territory,bool same,
            bool school,bool pair,bool active,bool territorial)
        {socialType=type;recommendedMinimum=Mathf.Max(0,minimum);provisionalMaximum=Mathf.Max(recommendedMinimum,maximum);
         toleratesSolitary=solitary;territoriality=territory;coexistsWithSameSpecies=same;needsSchool=school;needsPair=pair;
         toleratesActiveSpecies=active;toleratesTerritorialSpecies=territorial;}
    }
    [Serializable] public sealed class AquariumHabitatProfile
    {
        public bool UpperZoneAvailable=true,MiddleZoneAvailable=true,LowerZoneAvailable=true,HidingPlaces=true,PlantCoverage=true;
        [Range(0,1)] public float OpenSwimmingSpace=.8f;
        public bool Supports(SwimmingLevel zone)=>zone==SwimmingLevel.Any||zone==SwimmingLevel.Upper&&UpperZoneAvailable||
            zone==SwimmingLevel.Middle&&MiddleZoneAvailable||zone==SwimmingLevel.Lower&&LowerZoneAvailable;
    }
}
