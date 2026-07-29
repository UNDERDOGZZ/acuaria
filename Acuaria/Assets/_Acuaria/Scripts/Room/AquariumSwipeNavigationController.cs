using Acuaria.Aquarium.MultiAquarium;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Acuaria.Room
{
    public sealed class AquariumSwipeNavigationController:MonoBehaviour
    {
        [SerializeField] AquariumManager manager;
        [SerializeField] AquariumNavigationCoordinator navigation;
        [SerializeField,Min(20f)] float minimumDistance=90f;
        [SerializeField,Min(1f)] float horizontalDominance=1.35f;
        [SerializeField,Min(.1f)] float maximumDuration=1.25f;
        [SerializeField,Min(0f)] float cooldown=.25f;
        Vector2 start;
        float startedAt,lastChange=-10f;
        bool tracking,blockedAtStart;

        public void Configure(AquariumManager value)=>manager=value;
        public void Configure(AquariumManager value,AquariumNavigationCoordinator coordinator)
        {manager=value;navigation=coordinator;}
        void Awake(){if(manager==null)manager=AquariumManager.Instance;}
        void Update()
        {
            var pointer=Pointer.current;
            if(pointer==null)return;
            if(pointer.press.wasPressedThisFrame)
            {
                start=pointer.position.ReadValue();startedAt=Time.unscaledTime;tracking=true;
                blockedAtStart=EventSystem.current!=null&&EventSystem.current.IsPointerOverGameObject();
            }
            if(pointer.press.wasReleasedThisFrame&&tracking)
            {
                tracking=false;
                if(blockedAtStart||Time.unscaledTime-lastChange<cooldown||manager==null)return;
                var result=AquariumSwipeGesture.Evaluate(start,pointer.position.ReadValue(),
                    Time.unscaledTime-startedAt,minimumDistance,horizontalDominance,maximumDuration);
                var changed=result switch
                {
                    AquariumSwipeResult.Next=>navigation!=null?navigation.RequestRelative(1):manager.ActivateNext(1),
                    AquariumSwipeResult.Previous=>navigation!=null?navigation.RequestRelative(-1):manager.ActivateNext(-1),
                    _=>false
                };
                if(changed)lastChange=Time.unscaledTime;
            }
        }
        void OnDisable()=>tracking=false;
    }
}
