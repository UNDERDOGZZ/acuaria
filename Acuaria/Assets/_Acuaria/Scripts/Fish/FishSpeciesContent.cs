using System;
using UnityEngine;

namespace Acuaria.Fish
{
    public enum SpeciesDataValidationStatus { Draft, NeedsReview, Reviewed, Verified }
    public enum SpeciesSourceType { Scientific, Veterinary, Institutional, AquariumAssociation, SpecialistReference, Other }
    public enum AquaticWaterType { Freshwater, Brackish, Marine }
    public enum FishDiscoveryState { Hidden, Silhouette, Discovered, Studied }

    [Serializable]
    public sealed class SpeciesSourceReference
    {
        [SerializeField] string sourceId,title,organisation,consultedOn,url,notes;
        [SerializeField] SpeciesSourceType sourceType;
        [SerializeField] string[] supportedFields=Array.Empty<string>();
        public string SourceId=>sourceId; public string Title=>title; public string Organisation=>organisation;
        public string ConsultedOn=>consultedOn; public string Url=>url; public string Notes=>notes;
        public SpeciesSourceType SourceType=>sourceType; public string[] SupportedFields=>supportedFields;
        public bool IsValid=>!string.IsNullOrWhiteSpace(sourceId)&&!string.IsNullOrWhiteSpace(title)&&
            !string.IsNullOrWhiteSpace(organisation)&&!string.IsNullOrWhiteSpace(url);
        public void Configure(string id,string sourceTitle,string owner,SpeciesSourceType type,string date,string address,
            string sourceNotes,params string[] fields)
        {sourceId=id;title=sourceTitle;organisation=owner;sourceType=type;consultedOn=date;url=address;notes=sourceNotes;
         supportedFields=fields??Array.Empty<string>();}
    }

    [Serializable]
    public sealed class FishBiologicalProfile
    {
        [SerializeField] string scientificName,alternateCommonName,family,originRegion,bodyShape,generalBehaviour,sexualDimorphism,reproductionNotes,notes;
        [SerializeField] AquaticWaterType waterType=AquaticWaterType.Freshwater;
        [SerializeField] Vector2 adultLengthCm=new(1,5),lifespanYears;
        [SerializeField] Care.FishActivityLevel activity=Care.FishActivityLevel.Moderate;
        [SerializeField] SwimmingLevel primaryZone=SwimmingLevel.Any;
        [SerializeField] SwimmingLevel[] secondaryZones=Array.Empty<SwimmingLevel>();
        public string ScientificName=>scientificName; public string Family=>family; public string OriginRegion=>originRegion;
        public AquaticWaterType WaterType=>waterType; public Vector2 AdultLengthCm=>adultLengthCm; public Vector2 LifespanYears=>lifespanYears;
        public Care.FishActivityLevel Activity=>activity; public SwimmingLevel PrimaryZone=>primaryZone;
        public string GeneralBehaviour=>generalBehaviour;
        public bool IsValid=>!string.IsNullOrWhiteSpace(scientificName)&&adultLengthCm.x>0&&
            adultLengthCm.y>=adultLengthCm.x&&lifespanYears.x>=0&&lifespanYears.y>=lifespanYears.x;
        public void Configure(string scientific,string familyName,string region,Vector2 length,Vector2 lifespan,
            Care.FishActivityLevel activityLevel,SwimmingLevel zone,string behaviour)
        {scientificName=scientific;family=familyName;originRegion=region;waterType=AquaticWaterType.Freshwater;
         adultLengthCm=new Vector2(Mathf.Max(.1f,length.x),Mathf.Max(length.x,length.y));
         lifespanYears=new Vector2(Mathf.Max(0,lifespan.x),Mathf.Max(lifespan.x,lifespan.y));
         activity=activityLevel;primaryZone=zone;generalBehaviour=behaviour;}
    }

    [Serializable]
    public sealed class FishEducationalProfile
    {
        [SerializeField,TextArea] string summary,behaviour,care,feeding,socialLife,compatibility,commonMistakes,funFact,
            beginnerTip,mainWarning,welfare,habitat,maintenance;
        [SerializeField] string[] relatedConcepts=Array.Empty<string>();
        [SerializeField] string codexEntryId;
        public string Summary=>summary; public string Behaviour=>behaviour; public string Care=>care; public string Feeding=>feeding;
        public string SocialLife=>socialLife; public string Compatibility=>compatibility; public string BeginnerTip=>beginnerTip;
        public string MainWarning=>mainWarning; public string CodexEntryId=>codexEntryId;
        public bool IsValid=>!string.IsNullOrWhiteSpace(summary)&&!string.IsNullOrWhiteSpace(care);
        public void Configure(string summaryText,string behaviourText,string careText,string feedingText,string socialText,
            string compatibilityText,string tip,string warning,string codexId)
        {summary=summaryText;behaviour=behaviourText;care=careText;feeding=feedingText;socialLife=socialText;
         compatibility=compatibilityText;beginnerTip=tip;mainWarning=warning;codexEntryId=codexId;}
    }

    [Serializable]
    public sealed class FishVisualDefinition
    {
        [SerializeField] FishView prefab;
        [SerializeField] Sprite primarySprite,alternateSprite,catalogIcon,detailImage;
        [SerializeField] Vector2 scaleRange=new(.75f,1),speedRange=new(.45f,.8f);
        [SerializeField] Color baseColor=Color.cyan;
        [SerializeField] float oscillationFrequency=1,tailAmplitude=.08f,approximateSize=1;
        [SerializeField] bool spriteFacesRight=true;
        [SerializeField] string sortingLayer="Fish";
        [SerializeField] int sortingOrder;
        public FishView Prefab=>prefab; public Sprite PrimarySprite=>primarySprite; public Sprite CatalogIcon=>catalogIcon;
        public Vector2 ScaleRange=>scaleRange; public Vector2 SpeedRange=>speedRange; public Color BaseColor=>baseColor;
        public float OscillationFrequency=>oscillationFrequency; public float TailAmplitude=>tailAmplitude;
        public float ApproximateSize=>approximateSize; public bool SpriteFacesRight=>spriteFacesRight;
        public string SortingLayer=>sortingLayer; public int SortingOrder=>sortingOrder;
        public bool IsValid=>scaleRange.x>0&&scaleRange.y>=scaleRange.x&&speedRange.x>0&&speedRange.y>=speedRange.x;
        public void Configure(FishView fishPrefab,Vector2 scale,Vector2 speed,Color color)
        {prefab=fishPrefab;scaleRange=new Vector2(Mathf.Max(.01f,scale.x),Mathf.Max(scale.x,scale.y));
         speedRange=new Vector2(Mathf.Max(.01f,speed.x),Mathf.Max(speed.x,speed.y));baseColor=color;}
    }
}
