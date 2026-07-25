using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
namespace Acuaria.UI.Maintenance
{
    public sealed class WaterChangeVisualController:MonoBehaviour
    {
        [SerializeField] private Image overlay;[SerializeField] private RectTransform waterVisual;
        private Vector2 originalSize;private Coroutine routine;
        public void Configure(Image veil,RectTransform water){overlay=veil;waterVisual=water;}
        public IEnumerator AnimateDrain(float duration,int percentage,Action<float> progress)
        {
            if(waterVisual!=null)originalSize=waterVisual.sizeDelta;
            yield return Animate(duration,0f,percentage,progress);
        }
        public IEnumerator AnimateRefill(float duration,int percentage,Action<float> progress)=>Animate(duration,1f,percentage,progress);
        private IEnumerator Animate(float duration,float direction,int percentage,Action<float> progress)
        {
            for(var elapsed=0f;elapsed<duration;elapsed+=Time.unscaledDeltaTime)
            {var t=Mathf.Clamp01(elapsed/Mathf.Max(0.01f,duration));var amount=direction<0.5f?t:1f-t;
             if(overlay!=null)overlay.color=new Color(0.2f,0.75f,0.9f,0.16f*amount);
             if(waterVisual!=null)waterVisual.sizeDelta=originalSize+new Vector2(0f,-originalSize.y*percentage/100f*0.35f*amount);
             progress?.Invoke(t);yield return null;}
            progress?.Invoke(1f);
        }
        public void Restore(){if(waterVisual!=null)waterVisual.sizeDelta=originalSize;if(overlay!=null)overlay.color=Color.clear;}
        private void OnDisable(){if(routine!=null)StopCoroutine(routine);Restore();}
    }
}
