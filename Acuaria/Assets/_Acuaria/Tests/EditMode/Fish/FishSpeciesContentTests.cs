using Acuaria.Fish;
using Acuaria.Fish.Care;
using NUnit.Framework;
using UnityEngine;

namespace Acuaria.Tests.Fish
{
    public sealed class FishSpeciesContentTests
    {
        readonly System.Collections.Generic.List<Object> cleanup=new();
        [TearDown]public void TearDown(){for(var i=0;i<cleanup.Count;i++)Object.DestroyImmediate(cleanup[i]);cleanup.Clear();}
        [Test]public void Registry_FindsStableIdsAndDetectsDuplicates()
        {var fish=Species("fish.test");var registry=New<FishSpeciesRegistry>();registry.Configure(fish,fish);
         Assert.That(registry.FindById("fish.test"),Is.SameAs(fish));Assert.That(registry.ValidateContent(),Has.Some.Contains("duplicada"));}
        [Test]public void Population_TotalIsDataDrivenAndNotLimitedToThree()
        {var fish=Species("fish.school");var population=New<AquariumPopulationDefinition>();
         population.Configure("population.test","Test",PopulationValidationStatus.Debug,new AquariumPopulationEntry(fish,6,10));
         Assert.That(population.TotalCount,Is.EqualTo(6));}
        [Test]public void Suitability_WorstConditionDominatesAndIsDeterministic()
        {var fish=Species("fish.suitable");var evaluator=new SpeciesTankSuitabilityEvaluator();var habitat=new AquariumHabitatProfile();
         var context=new SpeciesTankContext(5,10,1,habitat);var first=evaluator.Evaluate(fish,context);var second=evaluator.Evaluate(fish,context);
         Assert.That(first.Status,Is.EqualTo(SpeciesTankSuitability.Unsuitable));Assert.That(second.Status,Is.EqualTo(first.Status));}
        [Test]public void Discovery_OnlyAdvancesAndNeverDuplicatesRewards()
        {var tracker=new FishDiscoveryTracker();Assert.That(tracker.Advance("fish.test",FishDiscoveryState.Discovered),Is.True);
         Assert.That(tracker.Advance("fish.test",FishDiscoveryState.Discovered),Is.False);Assert.That(tracker.Advance("fish.test",FishDiscoveryState.Studied),Is.True);}
        [Test]public void VerifiedWithoutSources_IsDowngradedForReview()
        {var fish=Species("fish.review");fish.ConfigureContent(new FishBiologicalProfile(),new FishEducationalProfile(),new FishVisualDefinition(),
          SpeciesDataValidationStatus.Verified,System.Array.Empty<SpeciesSourceReference>(),"",1);Assert.That(fish.ValidationStatus,Is.EqualTo(SpeciesDataValidationStatus.NeedsReview));}
        FishSpeciesDefinition Species(string id)
        {var fish=New<FishSpeciesDefinition>();fish.Configure(id,"Test",new Vector2(.4f,.8f),new Vector2(.8f,1),new Vector2(2,4),0,Color.white,SwimmingLevel.Middle);
         var care=new FishCareRequirements();care.Configure(new Vector2(22,26),20,40,5,FishActivityLevel.Moderate,SwimmingLevel.Middle,
          FishWaterSensitivity.Moderate,false,true,Vector3.one,1,FishDietType.Omnivore);
         var social=new FishSocialRequirements();social.Configure(FishSocialType.Group,3,8,false,FishTerritoriality.Peaceful,true,false,false,true,false);
         fish.ConfigureCare(care,social,new Acuaria.Fish.Compatibility.FishCompatibilityProfile());return fish;}
        T New<T>()where T:ScriptableObject{var value=ScriptableObject.CreateInstance<T>();cleanup.Add(value);return value;}
    }
}
