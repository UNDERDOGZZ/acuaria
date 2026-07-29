using UnityEngine;

namespace Acuaria.Aquarium.Decorations
{
    public sealed class DecorationSelectionVisual:MonoBehaviour
    {
        [SerializeField] LineRenderer line;DecorationView target;
        public void Configure(){if(line==null)line=GetComponent<LineRenderer>();if(line==null)line=gameObject.AddComponent<LineRenderer>();line.useWorldSpace=true;line.loop=true;line.positionCount=4;line.startWidth=line.endWidth=.035f;line.material=new Material(Shader.Find("Sprites/Default"));Hide();}
        void LateUpdate(){if(target==null||line==null||!line.enabled)return;var b=target.Renderer.bounds;line.SetPosition(0,new(b.min.x,b.min.y,-.4f));line.SetPosition(1,new(b.min.x,b.max.y,-.4f));line.SetPosition(2,new(b.max.x,b.max.y,-.4f));line.SetPosition(3,new(b.max.x,b.min.y,-.4f));}
        public void Show(DecorationView value){target=value;if(line==null)Configure();line.enabled=value!=null;SetValidity(true);}
        public void SetValidity(bool valid){if(line!=null)line.startColor=line.endColor=valid?new Color(.2f,1,.55f):new Color(1,.25f,.2f);}
        public void Hide(){target=null;if(line!=null)line.enabled=false;}
    }
}
