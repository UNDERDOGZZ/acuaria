using Acuaria.Fish;
using UnityEngine;

namespace Acuaria.Room
{
    public sealed class AquariumViewBinding:MonoBehaviour
    {
        [SerializeField] string slotId;
        [SerializeField] Transform aquariumRoot;
        [SerializeField] Transform cameraFocusPoint;
        [SerializeField] FishSpawner2D fishSpawner;
        public string SlotId=>slotId;
        public Transform AquariumRoot=>aquariumRoot!=null?aquariumRoot:transform;
        public Transform CameraFocusPoint=>cameraFocusPoint!=null?cameraFocusPoint:transform;
        public FishSpawner2D FishSpawner=>fishSpawner;
        public void Configure(string id,Transform root,Transform focus,FishSpawner2D spawner)
        {slotId=id;aquariumRoot=root;cameraFocusPoint=focus;fishSpawner=spawner;}
    }
}
