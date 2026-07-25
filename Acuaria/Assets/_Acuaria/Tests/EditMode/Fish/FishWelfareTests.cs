using System.Collections.Generic;
using Acuaria.Fish.Care;
using Acuaria.Fish.Compatibility;
using Acuaria.Fish.Welfare;
using Acuaria.Simulation.Water;
using NUnit.Framework;
using UnityEngine;
namespace Acuaria.Fish.Tests
{
    public sealed class FishWelfareTests
    {
        FishSpeciesDefinition species;FishWelfareDefinition settings;FishCareRequirements care;FishSocialRequirements social;
        [SetUp] public void Setup(){species=ScriptableObject.CreateInstance<FishSpeciesDefinition>();species.Configure("test","Test Fish",new Vector2(.5f,.8f),Vector2.one,new Vector2(2,4),0,Color.cyan,SwimmingLevel.Middle);
            care=new FishCareRequirements();care.Configure(new Vector2(24,26),20,40,5,FishActivityLevel.Moderate,SwimmingLevel.Middle,FishWaterSensitivity.Moderate,false,false,Vector3.one,1,FishDietType.Omnivore);
            social=new FishSocialRequirements();social.Configure(FishSocialType.Group,2,6,true,FishTerritoriality.Peaceful,true,false,false,true,false);
            var compatibility=new FishCompatibilityProfile();compatibility.Configure(new Vector2(2,10),true,true,false,true,true,false,false);species.ConfigureCare(care,social,compatibility);
            settings=ScriptableObject.CreateInstance<FishWelfareDefinition>();settings.Configure("test",Vector4.one,Vector3.one,new Vector3(10,20,0),new Vector3(40,70,90),5);}
        [TearDown] public void Cleanup(){Object.DestroyImmediate(species);Object.DestroyImmediate(settings);}
        FishWelfareContext Context(float temp=25,float volume=50,int same=2,WaterQualityStatus water=WaterQualityStatus.Excellent,float satiety=.55f,
            FishCompatibilityStatus compatibility=FishCompatibilityStatus.Compatible,AquariumStockingStatus stocking=AquariumStockingStatus.Appropriate)=>
            new(temp,volume,satiety,same,water,compatibility,new AquariumHabitatProfile(),stocking);
        [Test] public void CareRequirements_AreValidAndFinite()=>Assert.IsTrue(care.IsValid);
        [Test] public void SocialRequirements_AreValid()=>Assert.IsTrue(social.IsValid);
        [Test] public void WelfareDefinition_IsValid()=>Assert.IsTrue(settings.IsValid);
        [Test] public void IdealConditions_ProduceHighScore()=>Assert.GreaterOrEqual(new FishWelfareEvaluator().Evaluate(species,Context(),settings).OverallScore,90);
        [Test] public void TemperatureOutsideRange_ReducesScore(){var e=new FishWelfareEvaluator();Assert.Less(e.Evaluate(species,Context(20),settings).TemperatureScore,e.Evaluate(species,Context(25),settings).TemperatureScore);}
        [Test] public void InsufficientVolume_ReducesScore()=>Assert.Less(new FishWelfareEvaluator().Evaluate(species,Context(volume:10),settings).VolumeScore,100);
        [Test] public void InsufficientGroup_CreatesIssue(){var r=new FishWelfareEvaluator().Evaluate(species,Context(same:1),settings);Assert.Less(r.SocialScore,100);Assert.IsNotEmpty(r.Recommendations);}
        [Test] public void DangerousWater_PenalizesSensitiveMore(){care.Configure(new Vector2(24,26),20,40,5,FishActivityLevel.Moderate,SwimmingLevel.Middle,FishWaterSensitivity.Sensitive,false,false,Vector3.one,1,FishDietType.Omnivore);var sensitive=new FishWelfareEvaluator().Evaluate(species,Context(water:WaterQualityStatus.Dangerous),settings);
            care.Configure(new Vector2(24,26),20,40,5,FishActivityLevel.Moderate,SwimmingLevel.Middle,FishWaterSensitivity.Hardy,false,false,Vector3.one,1,FishDietType.Omnivore);var hardy=new FishWelfareEvaluator().Evaluate(species,Context(water:WaterQualityStatus.Dangerous),settings);Assert.Less(sensitive.WaterQualityScore,hardy.WaterQualityScore);}
        [Test] public void Incompatibility_ReducesScore()=>Assert.Less(new FishWelfareEvaluator().Evaluate(species,Context(compatibility:FishCompatibilityStatus.Incompatible),settings).CompatibilityScore,50);
        [Test] public void EmptyAquarium_IsSpacious()=>Assert.AreEqual(AquariumStockingStatus.Spacious,new AquariumStockingModel().Evaluate(new List<FishSpeciesDefinition>(),50).Status);
        [Test] public void HighActivity_RaisesDemand(){var list=new List<FishSpeciesDefinition>{species};var model=new AquariumStockingModel();var initial=model.Evaluate(list,50).Demand;care.Configure(new Vector2(24,26),20,40,5,FishActivityLevel.High,SwimmingLevel.Middle,FishWaterSensitivity.Moderate,false,false,Vector3.one,1,FishDietType.Omnivore);Assert.Greater(model.Evaluate(list,50).Demand,initial);}
        [Test] public void ThreeSpecies_GenerateThreeUniquePairs(){var list=new List<FishSpeciesDefinition>{species,species,species};Assert.AreEqual(3,new AquariumCompatibilityReport(list,50).PairSummaries.Count);}
        [Test] public void Evolution_ApproachesTarget(){var state=new FishWelfareState();state.Initialize("fish",50);var evaluation=new FishWelfareEvaluator().Evaluate(species,Context(),settings);new FishWelfareSimulationModel().Step(state,evaluation,settings,1);Assert.Greater(state.CurrentScore,50);Assert.LessOrEqual(state.CurrentScore,evaluation.OverallScore);}
        [TestCase(FishWelfareStatus.Excellent,1f)][TestCase(FishWelfareStatus.Poor,.68f)] public void VisualAdapter_IsAbsolute(FishWelfareStatus status,float expected)=>Assert.AreEqual(expected,FishWelfareVisualAdapter.SpeedMultiplier(status),.001f);
        [Test] public void AquariumWelfare_ConsidersWorst(){var a=new FishWelfareState();a.Initialize("a",95);var b=new FishWelfareState();b.Initialize("b",20);Assert.Less(AquariumWelfareEvaluator.Evaluate(new[]{a,b}).Score,70);}
    }
}
