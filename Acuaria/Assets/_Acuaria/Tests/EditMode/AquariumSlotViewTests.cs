using System;
using NUnit.Framework;
using UnityEngine;

namespace Acuaria.Room.Tests
{
    public sealed class AquariumSlotViewTests
    {
        private GameObject slotObject;
        private GameObject contentRootObject;
        private AquariumSlotView slot;

        [SetUp]
        public void SetUp()
        {
            slotObject = new GameObject("Slot");
            contentRootObject = new GameObject("Content");
            contentRootObject.transform.SetParent(slotObject.transform);
            slot = slotObject.AddComponent<AquariumSlotView>();
            slot.Configure("slot-01", contentRootObject.transform);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(slotObject);
        }

        [Test]
        public void Configure_RejectsEmptyId()
        {
            Assert.Throws<ArgumentException>(() => slot.Configure(" ", contentRootObject.transform));
        }

        [Test]
        public void AssignAndClearView_UpdatesOccupiedStateSafely()
        {
            var view = new GameObject("Aquarium");

            slot.AssignView(view);

            Assert.That(slot.IsOccupied, Is.True);
            Assert.That(slot.CurrentView, Is.SameAs(view));
            Assert.That(view.transform.parent, Is.SameAs(contentRootObject.transform));

            slot.ClearView();

            Assert.That(slot.IsOccupied, Is.False);
            Assert.That(slot.CurrentView, Is.Null);
        }

        [Test]
        public void AssignView_RejectsNull()
        {
            Assert.Throws<ArgumentNullException>(() => slot.AssignView(null));
        }
    }
}
