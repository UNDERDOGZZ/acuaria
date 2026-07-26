using System.Text;
using Acuaria.Aquarium.Decorations;
using UnityEngine;
using UnityEngine.UI;

namespace Acuaria.UI.Habitat
{
    public sealed class DecorationCatalogPanel : MonoBehaviour
    {
        [SerializeField] DecorationRegistry registry;
        [SerializeField] Text catalogText, detailText;
        public void Configure(DecorationRegistry source, Text list, Text detail){registry=source;catalogText=list;detailText=detail;Render();}
        public void Render(){if(catalogText==null||registry==null)return;var b=new StringBuilder("CATÁLOGO DE DECORACIONES\n\n");for(var i=0;i<registry.Decorations.Count;i++){var d=registry.Decorations[i];if(d!=null)b.AppendLine($"• {d.DisplayName} · {d.Category}");}catalogText.text=b.ToString();}
        public void Show(string id){var d=registry?.FindById(id);if(d==null||detailText==null)return;var favoured=new StringBuilder();for(var i=0;i<d.FavouredSpecies.Count;i++)if(d.FavouredSpecies[i]!=null)favoured.Append(i==0?"":", ").Append(d.FavouredSpecies[i].DisplayName);
            detailText.text=$"{d.DisplayName}\n\n{d.Description}\n\nBeneficios: plantas {d.Contribution.PlantCoverage:P0}, refugios {d.Contribution.HidingPlaces:0.#}, complejidad {d.Contribution.VisualComplexity:P0}.\nEspecies favorecidas: {(favoured.Length>0?favoured.ToString():"beneficio general")}\n\nDato educativo: {d.EducationalText}";}
    }
}
