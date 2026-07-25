using Acuaria.Progression;
using NUnit.Framework;
using UnityEngine;
namespace Acuaria.Aquarium.Tests
{
    public sealed class ProgressionTests
    {
        MissionDefinition mission;AchievementDefinition achievement;CodexEntry entry;
        [SetUp]public void Setup(){mission=ScriptableObject.CreateInstance<MissionDefinition>();mission.Configure("feed","Alimenta","Alimenta un pez",MissionType.Tutorial,ProgressionEventType.FishFed,2,25);
            achievement=ScriptableObject.CreateInstance<AchievementDefinition>();achievement.Configure("first","Primero","Haz algo",ProgressionEventType.FishFed,1,10);
            entry=ScriptableObject.CreateInstance<CodexEntry>();entry.Configure("ammonia","Amoníaco","Resumen","Explicación","Consejo",CodexCategory.Water);}
        [TearDown]public void Cleanup(){Object.DestroyImmediate(mission);Object.DestroyImmediate(achievement);Object.DestroyImmediate(entry);}
        [Test]public void Experience_RejectsNegativeAndAddsPositive(){var xp=new PlayerExperience();Assert.AreEqual(0,xp.Add(-5));Assert.AreEqual(25,xp.Add(25));Assert.AreEqual(25,xp.TotalXp);}
        [Test]public void Experience_LevelsWithoutFixedCap(){var level=PlayerExperience.CalculateLevel(100);Assert.AreEqual(2,level.Number);Assert.AreEqual("Aprendiz",level.Title);}
        [Test]public void Mission_CompletesAtTargetOnce(){var state=new MissionState();state.Initialize(mission);var evaluator=new MissionEvaluator();Assert.IsFalse(evaluator.Apply(mission,state,ProgressionEventType.FishFed));Assert.IsTrue(evaluator.Apply(mission,state,ProgressionEventType.FishFed));Assert.AreEqual(MissionStatus.Completed,state.Status);Assert.IsFalse(evaluator.Apply(mission,state,ProgressionEventType.FishFed));}
        [Test]public void MissionController_GrantsRewardOnce(){var player=new PlayerProgression();var controller=new MissionController(player,new[]{mission});controller.Process(ProgressionEventType.FishFed);controller.Process(ProgressionEventType.FishFed);controller.Process(ProgressionEventType.FishFed);Assert.AreEqual(25,player.Experience.TotalXp);}
        [Test]public void Codex_UnlockIsIdempotent(){var codex=new CodexController(new[]{entry});Assert.IsTrue(codex.Unlock("ammonia"));Assert.IsFalse(codex.Unlock("ammonia"));Assert.IsTrue(codex.States[0].IsUnlocked);}
        [Test]public void Achievement_UnlocksAndRewardsOnce(){var player=new PlayerProgression();var controller=new AchievementController(player,new[]{achievement});controller.Process(ProgressionEventType.FishFed);controller.Process(ProgressionEventType.FishFed);Assert.IsTrue(controller.States[0].IsUnlocked);Assert.AreEqual(10,player.Experience.TotalXp);}
        [Test]public void Statistics_RecordAllSupportedEvents(){var stats=new PlayerStatistics();var controller=new StatisticsController(stats);controller.Process(ProgressionEventType.FishFed);controller.Process(ProgressionEventType.WaterChanged);controller.Process(ProgressionEventType.FilterMaintained);controller.Process(ProgressionEventType.FoodWasted);Assert.AreEqual(1,stats.MealsGiven);Assert.AreEqual(1,stats.WaterChanges);Assert.AreEqual(1,stats.FilterCleanings);Assert.AreEqual(1,stats.WastedFood);}
        [Test]public void Statistics_TrackSimulatedExcellentTime(){var stats=new PlayerStatistics();stats.AddSimulation(2.5f,true,true);Assert.AreEqual(2.5,stats.SimulatedHours,.001);Assert.AreEqual(2.5,stats.ExcellentWaterHours,.001);Assert.AreEqual(2.5,stats.ExcellentWelfareHours,.001);}
        [Test]public void PlayerProgression_TracksGrantedXp(){var player=new PlayerProgression();player.GrantXp(40);Assert.AreEqual(40,player.Experience.TotalXp);Assert.AreEqual(40,player.Statistics.XpEarned);}
    }
}
