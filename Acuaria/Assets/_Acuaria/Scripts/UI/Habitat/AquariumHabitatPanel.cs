using System;
using System.Text;
using Acuaria.Aquarium.Decorations;
using UnityEngine;
using UnityEngine.UI;

namespace Acuaria.UI.Habitat
{
    public sealed class AquariumHabitatPanel : MonoBehaviour
    {
        [SerializeField] AquariumHabitatController habitat;
        [SerializeField] CanvasGroup group;
        [SerializeField] Text summary, explanation;
        [SerializeField] Button closeButton;
        public bool IsOpen { get; private set; }
        public void Configure(AquariumHabitatController source, CanvasGroup canvas, Text values, Text reasons, Button close)
        { habitat=source;group=canvas;summary=values;explanation=reasons;closeButton=close; }
        void OnEnable(){if(habitat!=null)habitat.Changed+=Render;closeButton?.onClick.AddListener(Close);Render(habitat?.CurrentProfile);}
        void OnDisable(){if(habitat!=null)habitat.Changed-=Render;closeButton?.onClick.RemoveListener(Close);}
        public void Open(){gameObject.SetActive(true);IsOpen=true;SetCanvas(true);Render(habitat?.CurrentProfile);}
        public void Close(){IsOpen=false;SetCanvas(false);gameObject.SetActive(false);}
        void SetCanvas(bool visible){if(group==null)return;group.alpha=visible?1:0;group.interactable=visible;group.blocksRaycasts=visible;}
        void Render(Acuaria.Fish.Care.AquariumHabitatProfile p)
        {
            if(p==null)return;var rating=Rating(p.OverallScore);
            if(summary!=null)summary.text=$"Cobertura vegetal  {p.PlantCoverageAmount:P0}\nRefugios  {p.HidingPlaceCount:0.#}\nEspacio libre  {p.OpenSwimmingSpace:P0}\nComplejidad  {p.VisualComplexity:P0}\n\nCalificación: {rating}\n\n{InstalledSummary()}";
            if(explanation!=null)explanation.text=Explain(p,rating);
        }
        static HabitatRating Rating(float score)=>score>=80?HabitatRating.Excellent:score>=60?HabitatRating.Good:score>=35?HabitatRating.Attention:HabitatRating.Poor;
        static string Explain(Acuaria.Fish.Care.AquariumHabitatProfile p,HabitatRating rating)
        {var b=new StringBuilder($"{rating}: ");if(p.PlantCoverageAmount<.2f)b.Append("faltan plantas; ");if(p.HidingPlaceCount<1)b.Append("faltan escondites; ");if(p.OpenSwimmingSpace<.45f)b.Append("queda poco espacio abierto; ");if(p.VisualComplexity<.2f)b.Append("el entorno es poco complejo; ");if(b.Length<15)b.Append("el hábitat ofrece variedad y espacio equilibrados.");return b.ToString();}
        string InstalledSummary()
        {
            if(habitat==null)return string.Empty;
            var counts=new System.Collections.Generic.Dictionary<DecorationDefinition,int>();
            for(var i=0;i<habitat.Placements.Count;i++){var d=habitat.Placements[i]?.Definition;if(d!=null)counts[d]=counts.TryGetValue(d,out var n)?n+1:1;}
            var b=new StringBuilder("Instaladas:\n");foreach(var pair in counts)
            {var c=pair.Key.Contribution;b.Append($"{pair.Key.DisplayName} x{pair.Value} · {pair.Key.Category}\n");if(c.PlantCoverage>0)b.Append($"  Cobertura +{c.PlantCoverage*pair.Value:P0}\n");if(c.HidingPlaces>0)b.Append($"  Refugios +{c.HidingPlaces*pair.Value:0.#}\n");}
            return b.ToString();
        }
    }

}
