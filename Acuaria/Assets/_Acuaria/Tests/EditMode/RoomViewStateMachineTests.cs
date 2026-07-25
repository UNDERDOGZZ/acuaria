using NUnit.Framework;
using UnityEngine;

namespace Acuaria.Room.Tests
{
    public sealed class RoomViewStateMachineTests
    {
        [Test]
        public void ValidFlow_ReturnsToOverview()
        {
            var machine = new RoomViewStateMachine();
            Assert.That(machine.State, Is.EqualTo(RoomViewState.RoomOverview));
            Assert.That(machine.TryBeginFocus(), Is.True);
            Assert.That(machine.IsTransitioning, Is.True);
            Assert.That(machine.TryCompleteFocus(), Is.True);
            Assert.That(machine.TryBeginReturn(), Is.True);
            Assert.That(machine.TryCompleteReturn(), Is.True);
            Assert.That(machine.State, Is.EqualTo(RoomViewState.RoomOverview));
        }

        [Test]
        public void InvalidAndDuplicateTransitions_AreRejected()
        {
            var machine = new RoomViewStateMachine();
            Assert.That(machine.TryCompleteFocus(), Is.False);
            Assert.That(machine.TryBeginFocus(), Is.True);
            Assert.That(machine.TryBeginFocus(), Is.False);
            Assert.That(machine.TryBeginReturn(), Is.False);
        }

        [Test]
        public void FocusTarget_UsesConfiguredPointAndPositiveSize()
        {
            var targetObject = new GameObject("Target");
            var pointObject = new GameObject("Point");

            try
            {
                pointObject.transform.position = new Vector3(2f, -1f, 0f);
                var target = targetObject.AddComponent<AquariumFocusTarget>();
                target.Configure("slot-test", pointObject.transform, -10f);

                Assert.That(target.SlotId, Is.EqualTo("slot-test"));
                Assert.That(target.Position, Is.EqualTo(pointObject.transform.position));
                Assert.That(target.OrthographicSize, Is.GreaterThan(0f));
                Assert.That(float.IsNaN(target.OrthographicSize), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(pointObject);
                Object.DestroyImmediate(targetObject);
            }
        }

        [TestCase(true, "slot-test", true)]
        [TestCase(false, "slot-test", false)]
        [TestCase(true, "", false)]
        public void Interactable_OnlyAllowsAvailableTargetsWithValidIds(
            bool available,
            string slotId,
            bool expected)
        {
            var aquariumObject = new GameObject("Aquarium");

            try
            {
                aquariumObject.AddComponent<BoxCollider2D>();
                var target = aquariumObject.AddComponent<AquariumFocusTarget>();
                target.Configure(slotId, aquariumObject.transform, 3f);
                var interactable = aquariumObject.AddComponent<AquariumInteractable>();
                interactable.Configure(target, available);

                Assert.That(interactable.IsSelectable, Is.EqualTo(expected));
            }
            finally
            {
                Object.DestroyImmediate(aquariumObject);
            }
        }
    }
}
