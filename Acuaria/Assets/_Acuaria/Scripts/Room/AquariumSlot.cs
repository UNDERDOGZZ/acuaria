using System;
using Acuaria.Aquarium.MultiAquarium;

namespace Acuaria.Room
{
    public enum AquariumSlotState { Locked, Empty, Occupied }
    [Serializable]
    public sealed class AquariumSlot
    {
        public string SlotId { get; }
        public AquariumSlotState State { get; private set; }
        public AquariumInstance Aquarium { get; private set; }
        public AquariumSlot(string id, AquariumSlotState state = AquariumSlotState.Empty)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Slot ID is required."); SlotId = id; State = state;
        }
        public bool Assign(AquariumInstance aquarium)
        {
            if (aquarium == null || State == AquariumSlotState.Locked) return false;
            Aquarium = aquarium; State = AquariumSlotState.Occupied; return true;
        }
        public bool Clear()
        {
            if (State != AquariumSlotState.Occupied) return false;
            Aquarium = null; State = AquariumSlotState.Empty; return true;
        }
        public void SetLocked(bool locked)
        {
            if (Aquarium != null && locked) return; State = locked ? AquariumSlotState.Locked : AquariumSlotState.Empty;
        }
    }
}
