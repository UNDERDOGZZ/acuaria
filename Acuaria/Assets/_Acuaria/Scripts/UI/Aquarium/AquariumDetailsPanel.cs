using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Acuaria.UI.Aquarium
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class AquariumDetailsPanel : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform panel;
        [SerializeField] private Button closeButton;
        [SerializeField] private Text titleLabel;
        [SerializeField] private Text summaryLabel;
        [SerializeField] private Text inhabitantsLabel;
        [SerializeField] private Text statusLabel;
        [SerializeField] private Text educationLabel;
        [SerializeField] private Text waterChemistryLabel;
        [SerializeField, Range(0.15f, 0.3f)] private float transitionDuration = 0.2f;
        private Coroutine transition;
        public bool IsOpen { get; private set; }

        public void Configure(CanvasGroup group, RectTransform panelRect, Button close, Text title, Text summary,
            Text inhabitants, Text status, Text education, Text chemistry = null)
        {
            canvasGroup = group;
            panel = panelRect;
            closeButton = close;
            titleLabel = title;
            summaryLabel = summary;
            inhabitantsLabel = inhabitants;
            statusLabel = status;
            educationLabel = education;
            waterChemistryLabel = chemistry;
        }

        private void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            SetImmediate(false);
        }

        private void OnEnable() => closeButton?.onClick.AddListener(Close);
        private void OnDisable() => closeButton?.onClick.RemoveListener(Close);

        public void Show(Acuaria.Aquarium.AquariumViewModel model)
        {
            if (model == null || IsOpen) return;
            titleLabel.text = model.DisplayName;
            summaryLabel.text = $"Volumen  {model.VolumeText}\nTemperatura  {model.TemperatureText}\n" +
                                $"Rango recomendado  {model.TemperatureRangeText}\n" +
                                $"Capacidad provisional  {model.CapacityText}\n\n" +
                                Acuaria.Aquarium.AquariumUIText.VolumeExplanation;
            inhabitantsLabel.text = model.InhabitantsText;
            statusLabel.text = $"● {model.StatusLabel}\n{model.StatusDescription}";
            educationLabel.text = model.EducationTip;
            IsOpen = true;
            StartTransition(true);
        }

        public void SetWaterChemistry(Acuaria.Simulation.Water.WaterChemistryViewModel chemistry)
        {
            if (waterChemistryLabel != null)
                waterChemistryLabel.text = chemistry?.DetailsText ?? "Química del agua no disponible.";
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            StartTransition(false);
        }

        public void CloseImmediate()
        {
            IsOpen = false;
            if (transition != null) StopCoroutine(transition);
            transition = null;
            SetImmediate(false);
        }

        private void StartTransition(bool opening)
        {
            if (transition != null) StopCoroutine(transition);
            if (opening) canvasGroup.gameObject.SetActive(true);
            transition = StartCoroutine(Transition(opening));
        }

        private IEnumerator Transition(bool opening)
        {
            canvasGroup.blocksRaycasts = false;
            var startAlpha = canvasGroup.alpha;
            var endAlpha = opening ? 1f : 0f;
            var startPosition = panel.anchoredPosition;
            var endPosition = new Vector2(0f, opening ? 0f : -18f);
            var elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / transitionDuration));
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, progress);
                panel.anchoredPosition = Vector2.Lerp(startPosition, endPosition, progress);
                yield return null;
            }
            canvasGroup.alpha = endAlpha;
            panel.anchoredPosition = endPosition;
            canvasGroup.interactable = opening;
            canvasGroup.blocksRaycasts = opening;
            transition = null;
        }

        private void SetImmediate(bool visible)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
            panel.anchoredPosition = visible ? Vector2.zero : new Vector2(0f, -18f);
        }
    }
}
