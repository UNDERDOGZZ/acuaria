using System;
using System.Collections;
using UnityEngine;

namespace Acuaria.Room
{
    public sealed class AquariumCameraCarouselController:MonoBehaviour
    {
        [SerializeField] Camera roomCamera;
        [SerializeField] AquariumCarouselDefinition definition;
        Coroutine transition;
        public bool IsTransitioning=>transition!=null;
        public event Action<int> TransitionCompleted;
        public void Configure(Camera camera,AquariumCarouselDefinition settings){roomCamera=camera;definition=settings;}
        public bool MoveTo(int index,Vector3 destination,int slotDistance)
        {
            if(roomCamera==null||definition==null||transition!=null)return false;
            transition=StartCoroutine(Animate(index,destination,definition.DurationForDistance(slotDistance)));
            return true;
        }
        IEnumerator Animate(int index,Vector3 destination,float duration)
        {
            var start=roomCamera.transform.position;
            var end=new Vector3(destination.x,destination.y,start.z);
            var elapsed=0f;
            while(elapsed<duration)
            {
                elapsed+=Time.unscaledDeltaTime;
                var t=definition.Curve.Evaluate(Mathf.Clamp01(elapsed/duration));
                roomCamera.transform.position=Vector3.LerpUnclamped(start,end,t);
                yield return null;
            }
            roomCamera.transform.position=end;
            transition=null;
            TransitionCompleted?.Invoke(index);
        }
        void OnDisable()
        {
            if(transition!=null)StopCoroutine(transition);
            transition=null;
        }
    }
}
