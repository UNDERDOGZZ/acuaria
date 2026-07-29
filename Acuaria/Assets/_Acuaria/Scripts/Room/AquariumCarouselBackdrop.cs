using UnityEngine;

namespace Acuaria.Room
{
    public sealed class AquariumCarouselBackdrop:MonoBehaviour
    {
        [SerializeField] Vector3 baseScale;
        [SerializeField] Vector3 basePosition;
        [SerializeField] bool initialized;
        public void Expand(int slotCount,float spacing)
        {
            if(!initialized){baseScale=transform.localScale;basePosition=transform.localPosition;initialized=true;}
            var count=Mathf.Max(1,slotCount);
            transform.localScale=new Vector3(baseScale.x*Mathf.Max(1f,count+.5f),baseScale.y,baseScale.z);
            transform.localPosition=new Vector3(basePosition.x+spacing*(count-1)*.5f,basePosition.y,basePosition.z);
        }
    }
}
