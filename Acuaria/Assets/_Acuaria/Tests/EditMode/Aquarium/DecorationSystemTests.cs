using System.Collections.Generic;
using Acuaria.Aquarium;
using Acuaria.Aquarium.Decorations;
using Acuaria.Fish;
using Acuaria.Fish.Care;
using Acuaria.Fish.Compatibility;
using Acuaria.Fish.Welfare;
using Acuaria.Simulation.Water;
using NUnit.Framework;
using UnityEngine;

namespace Acuaria.Tests.EditMode.Aquarium
{
    public sealed class DecorationSystemTests
    {
        [Test] public void Placement_ClampsInvalidCoordinatesAndScale()
        {
            var definition=Decoration("plant.safe","Planta",DecorationCategory.Plant,new HabitatContribution());
            var placement=new DecorationPlacementData("instance-1",definition,new Vector2(float.NaN,2),new Vector2(0,-2));
            Assert.That(placement.InstanceId,Is.EqualTo("instance-1"));Assert.That(placement.NormalizedPosition,Is.EqualTo(new Vector2(.5f,1)));
            Assert.That(placement.LocalScale.x,Is.GreaterThanOrEqualTo(.1f));Assert.That(placement.LocalScale.y,Is.EqualTo(2));
        }

        [Test] public void DecorationArea_ConvertsNormalizedBoundsSafely()
        {
            var go=new GameObject("area");var area=go.AddComponent<AquariumDecorationArea2D>();area.Configure(new Vector2(8,4),Vector2.zero);
            Assert.That(area.MinX,Is.LessThan(area.MaxX));Assert.That(area.MinY,Is.LessThan(area.MaxY));
            Assert.That(area.ToLocal(Vector2.zero),Is.EqualTo(new Vector2(-4,-2)));Assert.That(area.ToLocal(Vector2.one),Is.EqualTo(new Vector2(4,2)));
            Assert.That(area.Contains(area.ToLocal(new Vector2(.3f,.7f))),Is.True);Object.DestroyImmediate(go);
        }

        [Test] public void Spawner_SynchronizesByStableIdWithoutDuplicates()
        {
            var go=new GameObject("root");var area=go.AddComponent<AquariumDecorationArea2D>();area.Configure(new Vector2(8,4),Vector2.zero);
            var spawner=go.AddComponent<DecorationSpawner2D>();spawner.Configure(go.transform,area);
            var definition=Decoration("rock.visible","Roca",DecorationCategory.Rock,new HabitatContribution());
            var placement=new DecorationPlacementData("rock-1",definition,new Vector2(.4f,.1f),Vector2.one);
            spawner.SynchronizeInstalledDecorations(new[]{placement});spawner.SynchronizeInstalledDecorations(new[]{placement});
            Assert.That(spawner.Views.Count,Is.EqualTo(1));Assert.That(spawner.Views["rock-1"].transform.localPosition.x,Is.EqualTo(-.8f).Within(.001f));
            spawner.SynchronizeInstalledDecorations(System.Array.Empty<DecorationPlacementData>());Assert.That(spawner.Views,Is.Empty);Object.DestroyImmediate(go);
        }
        readonly List<Object> created = new();
        T New<T>() where T:ScriptableObject { var value=ScriptableObject.CreateInstance<T>();created.Add(value);return value; }
        [TearDown] public void TearDown(){for(var i=0;i<created.Count;i++)Object.DestroyImmediate(created[i]);created.Clear();}

        [Test] public void DecorationDefinition_IsValidAndPreservesContribution()
        {
            var item=Decoration("decoration.plant","Planta",DecorationCategory.Plant,new HabitatContribution(.4f,1,.1f,.2f,.2f,.3f));
            Assert.That(item.IsValid,Is.True);Assert.That(item.Contribution.PlantCoverage,Is.EqualTo(.4f).Within(.001f));
        }
        [Test] public void DecorationRegistry_FindsEntriesAndReportsDuplicateIds()
        {
            var item=Decoration("decoration.rock","Roca",DecorationCategory.Rock,new HabitatContribution(0,1,.1f,0,0,.2f));
            var registry=New<DecorationRegistry>();registry.Configure(item,item);
            Assert.That(registry.FindById("decoration.rock"),Is.SameAs(item));Assert.That(registry.ValidateContent(),Has.Some.Contains("duplicada"));
        }
        [Test] public void HabitatProfile_RecalculatesFunctionalValues()
        {
            var plant=Decoration("plant","Planta",DecorationCategory.Plant,new HabitatContribution(.35f,1,.1f,.1f,.2f,.3f));
            var cave=Decoration("cave","Cueva",DecorationCategory.Cave,new HabitatContribution(0,2,.15f,0,.1f,.3f));
            var profile=AquariumHabitatCalculator.Calculate(new[]{plant,cave});
            Assert.That(profile.PlantCoverageAmount,Is.EqualTo(.35f).Within(.001f));Assert.That(profile.HidingPlaceCount,Is.EqualTo(3));
            Assert.That(profile.OpenSwimmingSpace,Is.EqualTo(.75f).Within(.001f));Assert.That(profile.VisualComplexity,Is.EqualTo(.6f).Within(.001f));
        }
        [Test] public void AquariumDefinition_ExposesInstalledDecorationsAndCalculatedHabitat()
        {
            var aquarium=New<AquariumDefinition>();aquarium.Configure("test","Test",50,new Vector2(24,26),25,3,"","",Color.cyan);
            var plant=Decoration("plant","Planta",DecorationCategory.Plant,new HabitatContribution(.3f,1,.1f,0,0,.2f));
            aquarium.ConfigureDecorations(plant);
            Assert.That(aquarium.InstalledDecorations.Count,Is.EqualTo(1));Assert.That(aquarium.HabitatProfile.PlantCoverageAmount,Is.GreaterThan(0));
        }
        [Test] public void Habitat_AffectsWelfareOnlyForSpeciesRequirements()
        {
            var species=Species(needsHiding:true,needsPlants:true);var settings=New<FishWelfareDefinition>();
            settings.Configure("test",Vector4.one,Vector3.one,new Vector3(10,10,0),new Vector3(40,70,90),0);
            var evaluator=new FishWelfareEvaluator();
            var empty=new AquariumHabitatProfile(0,0,1,0,0,0);var natural=new AquariumHabitatProfile(.7f,2,.8f,.1f,.2f,.6f);
            var low=evaluator.Evaluate(species,Context(empty),settings);var high=evaluator.Evaluate(species,Context(natural),settings);
            Assert.That(high.OverallScore,Is.GreaterThan(low.OverallScore));Assert.That(low.ActiveIssues,Has.Some.Contains("escondites"));
        }
        [Test] public void HabitatValues_AreClampedAndFinite()
        {
            var item=Decoration("safe","Seguro",DecorationCategory.Artificial,new HabitatContribution(float.NaN,-2,4,float.PositiveInfinity,2,-1));
            var p=AquariumHabitatCalculator.Calculate(new[]{item});
            Assert.That(p.OpenSwimmingSpace,Is.InRange(0,1));Assert.That(float.IsFinite(p.OverallScore),Is.True);
        }

        DecorationDefinition Decoration(string id,string label,DecorationCategory category,HabitatContribution contribution)
        {var item=New<DecorationDefinition>();item.Configure(id,label,"Descripción",category,Vector2.one,.1f,contribution,"Dato educativo");return item;}
        FishSpeciesDefinition Species(bool needsHiding,bool needsPlants)
        {
            var s=New<FishSpeciesDefinition>();s.Configure("fish.habitat","Hábitat",new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(1,2),0,Color.white,SwimmingLevel.Middle);
            var care=new FishCareRequirements();care.Configure(new Vector2(24,26),10,10,5,FishActivityLevel.Moderate,SwimmingLevel.Middle,FishWaterSensitivity.Hardy,
                needsHiding,needsPlants,Vector3.one,1,FishDietType.Omnivore);care.ConfigureExtended(FishCareDifficulty.Beginner,0,FishEnvironmentLevel.Low,FishEnvironmentLevel.Moderate,.5f,.5f,true,"");
            var social=new FishSocialRequirements();social.Configure(FishSocialType.Solitary,1,1,true,FishTerritoriality.Peaceful,true,false,false,true,true);
            s.ConfigureCare(care,social,new FishCompatibilityProfile());return s;
        }
        static FishWelfareContext Context(AquariumHabitatProfile habitat)=>new(25,50,.6f,1,WaterQualityStatus.Excellent,FishCompatibilityStatus.Compatible,habitat,AquariumStockingStatus.Spacious);
    }
}
