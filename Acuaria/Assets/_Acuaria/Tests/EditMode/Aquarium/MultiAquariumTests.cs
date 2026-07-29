using Acuaria.Aquarium.MultiAquarium;
using NUnit.Framework;
using UnityEngine;
using Acuaria.UI.Aquarium;

namespace Acuaria.Tests.Aquarium
{
    public sealed class MultiAquariumTests
    {
        Acuaria.Aquarium.AquariumDefinition definition;
        [SetUp] public void SetUp()
        {
            definition = ScriptableObject.CreateInstance<Acuaria.Aquarium.AquariumDefinition>();
            definition.Configure("test-tank", "Test Tank", 50, new Vector2(24, 26), 25, 3, "", "", Color.cyan);
        }
        [TearDown] public void TearDown() => Object.DestroyImmediate(definition);

        [Test] public void Factory_CreatesIndependentStateGraphs()
        {
            var factory = new AquariumFactory();
            var first = factory.Create(definition, "first");
            var second = factory.Create(definition, "second");
            first.RuntimeState.SetTemperature(28);
            first.FishCollection.Add("fish-a");
            first.JournalState.Record("fed");
            Assert.That(second.RuntimeState.CurrentTemperature, Is.EqualTo(25));
            Assert.That(second.FishCollection.Count, Is.Zero);
            Assert.That(second.JournalState.Entries, Is.Empty);
            Assert.That(first.DecorationCollection, Is.Not.SameAs(second.DecorationCollection));
        }

        [Test] public void Repository_RejectsDuplicateIdsAndPreservesOrder()
        {
            var factory = new AquariumFactory(); var repository = new AquariumRepository();
            var first = factory.Create(definition, "first"); var second = factory.Create(definition, "second");
            Assert.That(repository.Register(first), Is.True);
            Assert.That(repository.Register(first), Is.False);
            Assert.That(repository.Register(second), Is.True);
            Assert.That(repository.All, Is.EqualTo(new[] { first, second }));
            Assert.That(repository.Find("second"), Is.SameAs(second));
        }

        [Test] public void Manager_ChangesExactlyOneActiveAquarium()
        {
            var go = new GameObject("AquariumManager-Test");
            try
            {
                var manager = go.AddComponent<AquariumManager>(); manager.ConfigureFactory(new AquariumFactory());
                var first = manager.CreateAquarium(definition, "first"); var second = manager.CreateAquarium(definition, "second");
                Assert.That(manager.ActiveAquarium, Is.SameAs(first));
                Assert.That(manager.Activate("second"), Is.True);
                Assert.That(first.RuntimeState.IsFocused, Is.False);
                Assert.That(second.RuntimeState.IsFocused, Is.True);
                Assert.That(manager.Context.Active, Is.SameAs(second));
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test] public void InactiveTick_DoesNotAdvanceActiveAquarium()
        {
            var go = new GameObject("AquariumManager-Test");
            try
            {
                var manager = go.AddComponent<AquariumManager>(); manager.ConfigureFactory(new AquariumFactory());
                var active = manager.CreateAquarium(definition, "active"); var inactive = manager.CreateAquarium(definition, "inactive");
                manager.TickInactive(30);
                Assert.That(active.NitrogenCycleState.SimplifiedSimulatedSeconds, Is.Zero);
                Assert.That(inactive.NitrogenCycleState.SimplifiedSimulatedSeconds, Is.EqualTo(30));
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test] public void Context_RaisesChangingThenChanged_AndKeepsExactlyOneActive()
        {
            var factory=new AquariumFactory();
            var first=factory.Create(definition,"first");
            var second=factory.Create(definition,"second");
            var context=new AquariumContext();
            var order=new System.Collections.Generic.List<string>();
            context.OnActiveAquariumChanging+=(oldValue,newValue)=>order.Add($"changing:{oldValue?.InstanceId}->{newValue.InstanceId}");
            context.OnActiveAquariumChanged+=(oldValue,newValue)=>order.Add($"changed:{oldValue?.InstanceId}->{newValue.InstanceId}");
            Assert.That(context.SetActiveAquarium(first),Is.True);
            Assert.That(context.SetActiveAquarium(second),Is.True);
            Assert.That(context.SetActiveAquarium(second),Is.False);
            Assert.That(first.IsActive,Is.False);
            Assert.That(second.IsActive,Is.True);
            Assert.That(order[^2],Is.EqualTo("changing:first->second"));
            Assert.That(order[^1],Is.EqualTo("changed:first->second"));
        }

        [Test] public void FishCollections_AreIndependent_AndRejectDuplicateIds()
        {
            var factory=new AquariumFactory();
            var first=factory.Create(definition,"first");
            var second=factory.Create(definition,"second");
            Assert.That(first.FishCollection.Add("fish-1"),Is.True);
            Assert.That(first.FishCollection.Add("fish-1"),Is.False);
            Assert.That(first.FishCollection.Count,Is.EqualTo(1));
            Assert.That(second.FishCollection.Count,Is.Zero);
        }

        [Test] public void HudFishCount_PrefersBoundAquariumOverLegacyProvider()
        {
            var aquarium=new AquariumFactory().Create(definition,"active");
            aquarium.FishCollection.Add("fish-1");
            Assert.That(AquariumHUDController.ResolveFishCount(aquarium,3),Is.EqualTo(1));
            aquarium.FishCollection.Remove("fish-1");
            Assert.That(AquariumHUDController.ResolveFishCount(aquarium,3),Is.Zero);
            Assert.That(AquariumHUDController.ResolveFishCount(null,3),Is.EqualTo(3));
        }
    }
}
