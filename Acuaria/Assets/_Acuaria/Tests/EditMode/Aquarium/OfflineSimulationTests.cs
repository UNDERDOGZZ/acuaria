using System;
using System.Collections.Generic;
using Acuaria.Offline;
using Acuaria.Save;
using NUnit.Framework;

namespace Acuaria.Aquarium.Tests
{
    public sealed class OfflineSimulationTests
    {
        static readonly DateTime Start=new(2026,7,28,10,0,0,DateTimeKind.Utc);
        [TestCase(60,OfflineTimeStatus.TooShort)]
        [TestCase(-60,OfflineTimeStatus.RollbackWithinTolerance)]
        public void TimeValidator_HandlesShortAndSmallRollback(int seconds,OfflineTimeStatus expected)
        {
            var data=Save(Start);var validation=new OfflineTimeValidator().Validate(data,Start.AddSeconds(seconds),OfflineSimulationPolicy.SafeDefault);
            Assert.That(validation.Status,Is.EqualTo(expected));Assert.That(validation.IsValid,Is.False);
        }
        [Test] public void TimeValidator_RejectsLargeRollback()
        {
            var validation=new OfflineTimeValidator().Validate(Save(Start),Start.AddHours(-2),OfflineSimulationPolicy.SafeDefault);
            Assert.That(validation.Status,Is.EqualTo(OfflineTimeStatus.ClockRollback));Assert.That(validation.IsValid,Is.False);
        }
        [Test] public void TimeValidator_CapsLongInterval()
        {
            var validation=new OfflineTimeValidator().Validate(Save(Start),Start.AddHours(72),OfflineSimulationPolicy.SafeDefault);
            Assert.That(validation.IsValid,Is.True);Assert.That(validation.WasCapped,Is.True);Assert.That(validation.Effective.TotalHours,Is.EqualTo(48));
        }
        [Test] public void Simulation_IsDeterministic()
        {
            var first=Save(Start);var second=Clone(first);
            var a=Service(Start.AddHours(12)).Simulate(first,OfflineSimulationPolicy.SafeDefault,true);
            var b=Service(Start.AddHours(12)).Simulate(second,OfflineSimulationPolicy.SafeDefault,true);
            Assert.That(a.Success&&b.Success,Is.True);
            Assert.That(a.Data.Aquariums[0].Fish[0].Hunger,Is.EqualTo(b.Data.Aquariums[0].Fish[0].Hunger));
            Assert.That(a.Data.Aquariums[0].WaterState.AmmoniaPpm,Is.EqualTo(b.Data.Aquariums[0].WaterState.AmmoniaPpm));
        }
        [Test] public void Simulation_IsIdempotentForSameInterval()
        {
            var data=Save(Start);var service=Service(Start.AddHours(10));var first=service.Simulate(data,OfflineSimulationPolicy.SafeDefault,true);
            var hunger=first.Data.Aquariums[0].Fish[0].Hunger;var sessions=first.Data.GlobalStatistics.OfflineSessions;
            var second=service.Simulate(first.Data,OfflineSimulationPolicy.SafeDefault,true);
            Assert.That(second.WasAlreadyApplied,Is.True);Assert.That(second.Data.Aquariums[0].Fish[0].Hunger,Is.EqualTo(hunger));
            Assert.That(second.Data.GlobalStatistics.OfflineSessions,Is.EqualTo(sessions));
        }
        [Test] public void Simulation_NeverKillsFishAndClampsHealth()
        {
            var result=Service(Start.AddDays(30)).Simulate(Save(Start),OfflineSimulationPolicy.SafeDefault,true);
            var fish=result.Data.Aquariums[0].Fish[0];
            Assert.That(fish.IsAlive,Is.True);Assert.That(fish.Health,Is.GreaterThanOrEqualTo(OfflineSimulationPolicy.SafeDefault.MinHealth));
            Assert.That(fish.Hunger,Is.InRange(0,1));
        }
        [Test] public void EmptyAquarium_DoesNotGenerateHungerOrWaste()
        {
            var data=Save(Start);data.Aquariums[0].Fish.Clear();
            var result=Service(Start.AddHours(12)).Simulate(data,OfflineSimulationPolicy.SafeDefault,true);
            Assert.That(result.Report.FishProcessed,Is.Zero);Assert.That(result.Data.Aquariums[0].WaterState.AmmoniaPpm,Is.Zero);
        }
        [Test] public void ThreeAquariums_RemainIndependent()
        {
            var data=Save(Start);data.Aquariums.Add(Aquarium("aq-2",1));data.Aquariums.Add(Aquarium("aq-3",0));
            var result=Service(Start.AddHours(12)).Simulate(data,OfflineSimulationPolicy.SafeDefault,true);
            Assert.That(result.Report.AquariumsProcessed,Is.EqualTo(3));Assert.That(result.Report.FishProcessed,Is.EqualTo(2));
            Assert.That(data.Aquariums[2].WaterState.AmmoniaPpm,Is.Zero);
        }
        [Test] public void Events_AreConsolidatedAndLimited()
        {
            var events=new[]{new OfflineEvent{Key="water",AquariumId="a",Priority=OfflineEventPriority.Info},
                new OfflineEvent{Key="water",AquariumId="a",Priority=OfflineEventPriority.Warning},
                new OfflineEvent{Key="filter",AquariumId="a",Priority=OfflineEventPriority.Warning}};
            var result=new OfflineEventAggregator().Aggregate(events,1);
            Assert.That(result.Count,Is.EqualTo(1));Assert.That(result[0].Priority,Is.EqualTo(OfflineEventPriority.Warning));
        }
        [Test] public void MigrationV1ToV2_UsesUpdatedTimestampNotCreation()
        {
            var data=Save(Start);data.SchemaVersion=1;data.CreatedAtUtc=Start.AddYears(-1).ToString("O");data.UpdatedAtUtc=Start.ToString("O");
            var migrated=new SaveMigrationV1ToV2().Migrate(data);
            Assert.That(migrated.LastSimulationAtUtc,Is.EqualTo(Start.ToString("O")));Assert.That(migrated.Aquariums.Count,Is.EqualTo(1));
        }
        static OfflineSimulationService Service(DateTime now)=>new(new FixedOfflineTimeProvider(now));
        static AcuariaSaveData Clone(AcuariaSaveData data)=>new JsonUtilitySaveSerializer().Deserialize(new JsonUtilitySaveSerializer().Serialize(data));
        static AcuariaSaveData Save(DateTime timestamp)
        {
            var data=new AcuariaSaveData{SaveId="save",CreatedAtUtc=timestamp.ToString("O"),UpdatedAtUtc=timestamp.ToString("O"),
                LastSimulationAtUtc=timestamp.ToString("O"),ActiveAquariumId="aq-1"};
            data.Aquariums.Add(Aquarium("aq-1",1));return data;
        }
        static AquariumSaveData Aquarium(string id,int fishCount)
        {
            var aquarium=new AquariumSaveData{AquariumInstanceId=id,AquariumDefinitionId="tank",DisplayName=id,IsInitialized=true,
                RuntimeState=new AquariumRuntimeSaveData{VolumeLiters=50,TemperatureCelsius=25},
                WaterState=new WaterStateSaveData{AmmoniaBacteria=.5f,NitriteBacteria=.5f},
                FilterState=new FilterStateSaveData{Efficiency=1,IsRunning=true},WelfareState=new WelfareSaveData()};
            for(var i=0;i<fishCount;i++)aquarium.Fish.Add(new FishSaveData{FishInstanceId=$"{id}-fish-{i}",AquariumInstanceId=id,
                SpeciesDefinitionId="species",Satiety=.7f,Hunger=.3f,Health=1,Welfare=1,IsAlive=true});
            return aquarium;
        }
    }
}
