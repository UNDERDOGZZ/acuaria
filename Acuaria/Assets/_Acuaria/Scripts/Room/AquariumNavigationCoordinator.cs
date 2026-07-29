using Acuaria.Aquarium.MultiAquarium;
using UnityEngine;

namespace Acuaria.Room
{
    public sealed class AquariumNavigationCoordinator:MonoBehaviour
    {
        [SerializeField] AquariumManager manager;
        [SerializeField] AquariumCameraCarouselController cameraController;
        [SerializeField] AquariumViewBinding[] bindings;
        int currentIndex,pendingIndex=-1;
        bool subscribed;
        public bool IsBlocked=>pendingIndex>=0||cameraController==null||cameraController.IsTransitioning;
        public void Configure(AquariumManager source,AquariumCameraCarouselController camera,AquariumViewBinding[] views)
        {manager=source;cameraController=camera;bindings=views;}
        void Start()
        {
            if(manager==null)manager=AquariumManager.Instance;
            Subscribe();
            BindViews();
        }
        void Subscribe()
        {
            if(subscribed||cameraController==null)return;
            cameraController.TransitionCompleted+=Complete;
            subscribed=true;
        }
        void BindViews()
        {
            if(manager==null||bindings==null)return;
            var count=Mathf.Min(manager.Aquariums.Count,bindings.Length);
            for(var i=0;i<count;i++)bindings[i]?.FishSpawner?.BindStates(manager.Aquariums[i].FishCollection.Fish);
        }
        public bool Request(int index)
        {
            if(IsBlocked||bindings==null||index<0||index>=bindings.Length||bindings[index]==null)return false;
            if(index==currentIndex)return false;
            pendingIndex=index;
            if(cameraController.MoveTo(index,bindings[index].CameraFocusPoint.position,Mathf.Abs(index-currentIndex)))return true;
            pendingIndex=-1;return false;
        }
        public bool RequestRelative(int direction)=>Request(currentIndex+System.Math.Sign(direction));
        void Complete(int index)
        {
            currentIndex=index;pendingIndex=-1;
            if(manager!=null&&index<manager.Aquariums.Count)manager.Activate(manager.Aquariums[index].InstanceId);
        }
        public void RefreshVisualBindings()=>BindViews();
        void OnDestroy(){if(subscribed&&cameraController!=null)cameraController.TransitionCompleted-=Complete;}
    }
}
