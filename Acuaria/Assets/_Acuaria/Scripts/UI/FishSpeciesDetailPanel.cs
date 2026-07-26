using Acuaria.Fish;
using UnityEngine;
using UnityEngine.UI;

namespace Acuaria.UI
{
    public sealed class FishSpeciesDetailPanel : MonoBehaviour
    {
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Button closeButton;
        [SerializeField] Text title, body, suitability;
        public FishSpeciesViewModel Current { get; private set; }

        public void Configure(CanvasGroup group, Button close, Text heading, Text description, Text suitabilityText)
        { canvasGroup = group; closeButton = close; title = heading; body = description; suitability = suitabilityText; }

        void OnEnable() => closeButton?.onClick.AddListener(Close);
        void OnDisable() => closeButton?.onClick.RemoveListener(Close);
        public void Show(FishSpeciesViewModel model) { Current = model; SetVisible(model != null); }
        public void Close() { Current = null; SetVisible(false); }

        void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
            if (canvasGroup == null) return;
            canvasGroup.alpha = visible ? 1 : 0; canvasGroup.interactable = visible; canvasGroup.blocksRaycasts = visible;
            if (!visible || Current?.Species == null) return;
            var species = Current.Species;
            if (title != null) title.text = $"{species.DisplayName}\n<i>{species.ScientificName}</i>";
            if (body != null)
                body.text = $"{species.ShortDescription}\n\nTemperatura: {species.Care.TemperatureRange.x:0}–{species.Care.TemperatureRange.y:0} °C\n" +
                    $"Volumen orientativo: {species.Care.MinimumIndividualVolume:0} L · Grupo: {species.Social.RecommendedMinimum}\n" +
                    $"Zona: {species.SwimmingLevel} · Dieta: {species.Care.Diet} · Dificultad: {species.Care.Difficulty}\n\n" +
                    $"{species.EducationalProfile.Summary}\n\nConsejo: {species.EducationalProfile.BeginnerTip}\nAdvertencia: {species.EducationalProfile.MainWarning}";
            if (suitability != null)
                suitability.text = "En tu acuario: consulta educativa; la adecuación depende de temperatura, volumen, grupo y hábitat.";
        }
    }
}
