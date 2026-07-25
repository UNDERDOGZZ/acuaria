using Acuaria.Simulation.Filtration;
using Acuaria.Simulation.Maintenance;
using Acuaria.Simulation.Water;
using NUnit.Framework;
using UnityEngine;

namespace Acuaria.Aquarium.Tests
{
    public sealed class MaintenanceSystemTests
    {
        AquariumMaintenanceDefinition maintenance;WaterChemistryDefinition chemistry;FilterDefinition filter;
        [SetUp] public void Setup()
        {
            maintenance=ScriptableObject.CreateInstance<AquariumMaintenanceDefinition>();
            maintenance.Configure("test",new[]{10,25,40,50},25,4f,0.8f,new Vector3(1f,1f,1f),new Vector2(20f,10f));
            chemistry=ScriptableObject.CreateInstance<WaterChemistryDefinition>();
            chemistry.Configure("water",50f,new Vector3(1f,2f,20f),new Vector2(.6f,.7f),new Vector2(.1f,.1f),
                new Vector2(.01f,.001f),new Vector3(.1f,.1f,.01f),new Vector2(200f,1000f),new Vector2(1f,60f),5,.001f,WaterQualityThresholds.Default);
            filter=ScriptableObject.CreateInstance<FilterDefinition>();
            filter.Configure("filter","Starter",new Vector2(30,70),.9f,.4f,.01f,.7f,100f,.05f,FilterType.Internal,"Education");
        }
        [TearDown] public void Cleanup(){Object.DestroyImmediate(maintenance);Object.DestroyImmediate(chemistry);Object.DestroyImmediate(filter);}
        WaterChemistryState State(){var state=new WaterChemistryState();state.Initialize("state",chemistry);state.SetDevelopmentValues(1,2,20,.6f,.7f,10,chemistry);return state;}
        [Test] public void Definition_HasRequiredOptionsAndRecommendation(){Assert.IsTrue(maintenance.IsValid);CollectionAssert.AreEqual(new[]{10,25,40,50},maintenance.AllowedPercentages);Assert.AreEqual(25,maintenance.RecommendedPercentage);}
        [TestCase(10,.9f)][TestCase(25,.75f)][TestCase(50,.5f)] public void WaterChange_ReducesConcentrations(int percent,float remaining)
        {var r=new WaterChangeModel().Calculate(State(),maintenance,percent);Assert.IsTrue(r.IsValid);Assert.AreEqual(20f*remaining,r.Nitrate,.001f);Assert.AreEqual(.6f,r.AmmoniaBacteria);Assert.AreEqual(.7f,r.NitriteBacteria);}
        [TestCase(0)][TestCase(-10)][TestCase(51)] public void WaterChange_RejectsInvalidPercentages(int percent)=>Assert.IsFalse(new WaterChangeModel().Calculate(State(),maintenance,percent).IsValid);
        [Test] public void Preview_MatchesFinalModel(){var state=State();var result=new WaterChangeModel().Calculate(state,maintenance,25);var vm=new AquariumMaintenanceViewModel(maintenance,state,25,null,new AquariumMaintenanceState());StringAssert.Contains($"{result.Nitrate:0.0}",vm.Preview);StringAssert.Contains("RECOMENDADO",vm.Preview);}
        [Test] public void MaintenanceState_BlocksDoubleBeginAndTracksCompletion(){var state=new AquariumMaintenanceState();state.Initialize("m");Assert.IsTrue(state.Begin(25,maintenance));Assert.IsFalse(state.Begin(25,maintenance));state.Complete(4);Assert.AreEqual(1,state.ChangesPerformed);Assert.AreEqual(AquariumMaintenanceResult.Success,state.LastResult);}
        [Test] public void Filter_DirtReducesEfficiencyDeterministically(){var a=new FilterRuntimeState();var b=new FilterRuntimeState();a.Initialize("a",filter);b.Initialize("b",filter);var model=new FilterSimulationModel();model.Step(a,filter,50,20,10);model.Step(b,filter,50,20,10);Assert.Less(a.CurrentEfficiency,filter.BaseEfficiency);Assert.AreEqual(a.CurrentEfficiency,b.CurrentEfficiency);Assert.LessOrEqual(a.DirtLevel,1f);}
        [Test] public void Filter_OffAddsNoBiologicalCapacity(){var state=new FilterRuntimeState();state.Initialize("f",filter);state.SetActive(false);new FilterSimulationModel().Step(state,filter,50,0,1);Assert.AreEqual(0f,state.CurrentEfficiency);Assert.AreEqual(FilterOperatingStatus.Off,state.Status);}
        [Test] public void GentleRinse_PreservesMoreBacteriaThanDeepClean(){var state=new FilterRuntimeState();state.Initialize("f",filter);var model=new FilterMaintenanceModel();var gentle=model.Calculate(state,FilterMaintenanceType.GentleRinse);var deep=model.Calculate(state,FilterMaintenanceType.DeepClean);Assert.Greater(gentle.BacteriaRetention,deep.BacteriaRetention);Assert.Greater(gentle.Dirt,deep.Dirt);}
        [Test] public void DeepClean_NeverCreatesNegativeValues(){var state=new FilterRuntimeState();state.Initialize("f",filter);var model=new FilterMaintenanceModel();Assert.IsTrue(model.Apply(state,filter,FilterMaintenanceType.DeepClean));Assert.GreaterOrEqual(state.DirtLevel,0);Assert.GreaterOrEqual(state.BiologicalCapacity,0);}
        [Test] public void NitrogenCycle_FilterMultiplierChangesConversion(){var state=State();var model=new NitrogenCycleModel();var low=model.Step(state,chemistry,3600,.5f);var high=model.Step(state,chemistry,3600,1.5f);Assert.Less(high.AmmoniaMgPerLiter,low.AmmoniaMgPerLiter);}
    }
}
