using Acuaria.Aquarium;
using Acuaria.Aquarium.MultiAquarium;
using Acuaria.Room;
using NUnit.Framework;
using UnityEngine;

namespace Acuaria.Tests
{
    public sealed class AquariumSlotModelTests
    {
        [Test] public void Slot_AssignsAndClearsAquarium()
        {
            var definition = ScriptableObject.CreateInstance<AquariumDefinition>();
            try
            {
                definition.Configure("tank", "Tank", 50, new Vector2(24, 26), 25, 3, "", "", Color.cyan);
                var aquarium = new AquariumFactory().Create(definition, "one");
                var slot = new AquariumSlot("slot-1");
                Assert.That(slot.Assign(aquarium), Is.True);
                Assert.That(slot.State, Is.EqualTo(AquariumSlotState.Occupied));
                Assert.That(slot.Clear(), Is.True);
                Assert.That(slot.State, Is.EqualTo(AquariumSlotState.Empty));
            }
            finally { Object.DestroyImmediate(definition); }
        }
    }
}
