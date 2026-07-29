using Acuaria.Aquarium.MultiAquarium;

namespace Acuaria.Room
{
    public sealed class AquariumSummaryViewModel
    {
        public string InstanceId{get;}
        public string DisplayName{get;}
        public int FishCount{get;}
        public float Temperature{get;}
        public bool IsActive{get;}
        public AquariumSummaryViewModel(AquariumInstance aquarium,AquariumContext context)
        {
            InstanceId=aquarium?.InstanceId;
            DisplayName=aquarium?.Name??"Acuario vacío";
            FishCount=aquarium?.FishCollection.Count??0;
            Temperature=aquarium?.RuntimeState.CurrentTemperature??0f;
            IsActive=ReferenceEquals(aquarium,context?.ActiveAquarium);
        }
    }
}
