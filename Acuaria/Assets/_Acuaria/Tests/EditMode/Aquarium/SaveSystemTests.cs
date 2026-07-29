using System;
using System.Collections.Generic;
using System.IO;
using Acuaria.Aquarium.MultiAquarium;
using Acuaria.Save;
using Acuaria.Simulation.Water;
using NUnit.Framework;
using UnityEngine;

namespace Acuaria.Aquarium.Tests
{
    public sealed class SaveSystemTests
    {
        readonly List<UnityEngine.Object> objects = new();
        string directory;
        SaveFileStorage storage;
        JsonUtilitySaveSerializer serializer;
        SaveService service;

        [SetUp] public void SetUp()
        {
            directory = Path.Combine(Path.GetTempPath(), "acuaria-save-tests-" + Guid.NewGuid().ToString("N"));
            storage = new SaveFileStorage(directory); serializer = new JsonUtilitySaveSerializer();
            service = new SaveService(serializer, storage, new SaveValidator(), new SaveMigrationPipeline());
        }
        [TearDown] public void TearDown()
        {
            foreach (var value in objects) if (value != null) UnityEngine.Object.DestroyImmediate(value);
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }

        [Test] public void JsonRoundTrip_PreservesRootAndAquariums()
        {
            var data = ValidSave();
            var json = serializer.Serialize(data);
            Assert.That(serializer.TryDeserialize(json, out var restored), Is.True);
            Assert.That(restored.SaveId, Is.EqualTo(data.SaveId));
            Assert.That(restored.Aquariums.Count, Is.EqualTo(1));
        }

        [Test] public void AtomicSave_CreatesMainThenBackup()
        {
            var first = ValidSave(); Assert.That(service.Save(first).Success, Is.True);
            first.UpdatedAtUtc = DateTime.UtcNow.AddSeconds(1).ToString("O");
            Assert.That(service.Save(first).Success, Is.True);
            Assert.That(File.Exists(storage.MainPath), Is.True);
            Assert.That(File.Exists(storage.BackupPath), Is.True);
            Assert.That(File.Exists(storage.TemporaryPath), Is.False);
        }

        [Test] public void CorruptMain_LoadsValidBackup()
        {
            var data = ValidSave(); Assert.That(service.Save(data).Success, Is.True);
            data.UpdatedAtUtc = DateTime.UtcNow.AddSeconds(1).ToString("O"); Assert.That(service.Save(data).Success, Is.True);
            File.WriteAllText(storage.MainPath, "{broken");
            var result = service.Load();
            Assert.That(result.Success, Is.True);
            Assert.That(result.Source, Is.EqualTo(SaveLoadSource.Backup));
        }

        [Test] public void FutureVersion_IsRejectedWithoutFallingBackOrOverwriting()
        {
            var data = ValidSave(); data.SchemaVersion = SaveSystemDefinition.CurrentSchemaVersion + 1;
            Directory.CreateDirectory(directory);
            File.WriteAllText(storage.MainPath, SaveIntegrity.Stamp(data, serializer));
            var before = File.ReadAllText(storage.MainPath);
            var result = service.Load();
            Assert.That(result.Success, Is.False);
            Assert.That(result.Validation.IsFutureVersion, Is.True);
            Assert.That(File.ReadAllText(storage.MainPath), Is.EqualTo(before));
        }

        [Test] public void Validator_RejectsDuplicateFishAndOrphanActiveAquarium()
        {
            var data = ValidSave(); var fish = data.Aquariums[0].Fish[0];
            data.Aquariums[0].Fish.Add(fish); data.ActiveAquariumId = "missing";
            var validation = new SaveValidator().Validate(data);
            Assert.That(validation.IsValid, Is.False);
        }

        [Test] public void Mapper_RestoresThreeAquariumsIdempotently()
        {
            var definition = Definition("tank", 50);
            var manager = Manager();
            for (var i = 1; i <= 3; i++)
            {
                var aquarium = manager.CreateAquarium(definition, $"aq-{i}", $"Tank {i}");
                aquarium.AssignPresentation($"slot-{i:00}", null);
                aquarium.FishCollection.Add($"fish-{i}");
            }
            manager.Activate("aq-3");
            var mapper = new SaveMapper(new[] { definition }, null, Chemistry());
            var data = mapper.Capture(manager, null);
            Assert.That(mapper.Apply(data, manager), Is.True);
            Assert.That(manager.Aquariums.Count, Is.EqualTo(3));
            Assert.That(manager.ActiveAquarium.InstanceId, Is.EqualTo("aq-3"));
            Assert.That(mapper.Apply(data, manager), Is.True);
            Assert.That(manager.Aquariums.Count, Is.EqualTo(3));
            Assert.That(manager.Aquariums[0].FishCollection.Count, Is.EqualTo(1));
            Assert.That(manager.ActiveAquarium.StatisticsState.Activations, Is.EqualTo(data.Aquariums[2].Statistics.AquariumVisits));
        }

        AcuariaSaveData ValidSave()
        {
            var now = DateTime.UtcNow.ToString("O");
            var data = new AcuariaSaveData { SaveId = "save-1", CreatedAtUtc = now, UpdatedAtUtc = now, ActiveAquariumId = "aq-1" };
            var aquarium = new AquariumSaveData
            {
                AquariumInstanceId = "aq-1", AquariumDefinitionId = "tank", DisplayName = "Tank", SlotId = "slot-01",
                RuntimeState = new AquariumRuntimeSaveData { TemperatureCelsius = 25, VolumeLiters = 50 }, IsInitialized = true
            };
            aquarium.Fish.Add(new FishSaveData { FishInstanceId = "fish-1", AquariumInstanceId = "aq-1", SpeciesDefinitionId = "species-1", Satiety = .5f });
            data.Aquariums.Add(aquarium);
            data.AquariumSlots.Add(new AquariumSlotSaveData { SlotId = "slot-01", SlotState = "Occupied", AssignedAquariumId = "aq-1" });
            return data;
        }
        AquariumDefinition Definition(string id, float volume)
        {
            var value = ScriptableObject.CreateInstance<AquariumDefinition>(); objects.Add(value);
            value.Configure(id, id, volume, new Vector2(24, 26), 25, 5, "", "", Color.cyan); return value;
        }
        WaterChemistryDefinition Chemistry()
        {
            var value = ScriptableObject.CreateInstance<WaterChemistryDefinition>(); objects.Add(value);
            value.Configure("chemistry", 50, new Vector3(0, 0, 5), new Vector2(.5f, .5f), new Vector2(.1f, .1f),
                new Vector2(.01f, .001f), new Vector3(.1f, .1f, .1f), new Vector2(200, 1000),
                new Vector2(1, 60), 5, .001f, WaterQualityThresholds.Default); return value;
        }
        AquariumManager Manager()
        {
            var go = new GameObject("manager"); objects.Add(go); return go.AddComponent<AquariumManager>();
        }
    }
}
