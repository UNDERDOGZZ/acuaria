namespace Acuaria.Aquarium
{
    public readonly struct AquariumInhabitant
    {
        public AquariumInhabitant(string speciesId, string displayName, string swimmingZone, int count)
        {
            SpeciesId = speciesId;
            DisplayName = displayName;
            SwimmingZone = swimmingZone;
            Count = count;
        }

        public string SpeciesId { get; }
        public string DisplayName { get; }
        public string SwimmingZone { get; }
        public int Count { get; }
    }
}
