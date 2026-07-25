using Acuaria.Aquarium;
using Acuaria.Food;
using UnityEngine;
using UnityEngine.UI;

namespace Acuaria.UI.Aquarium
{
    public sealed class AquariumHUDController : MonoBehaviour
    {
        [SerializeField] private AquariumDefinition definition;
        [SerializeField] private AquariumInhabitantProvider inhabitants;
        [SerializeField] private FeedingUIController feedingUi;
        [SerializeField] private AquariumDetailsPanel detailsPanel;
        [SerializeField] private GameObject compactHud;
        [SerializeField] private Button detailsButton;
        [SerializeField] private Text nameLabel;
        [SerializeField] private Text volumeLabel;
        [SerializeField] private Text temperatureLabel;
        [SerializeField] private Text fishCountLabel;
        [SerializeField] private Text statusLabel;
        [SerializeField] private Image statusBadge;
        private readonly AquariumRuntimeState runtimeState = new();
        private bool focused;

        public bool IsVisible => compactHud != null && compactHud.activeSelf;
        public bool AreDetailsOpen => detailsPanel != null && detailsPanel.IsOpen;
        public AquariumRuntimeState RuntimeState => runtimeState;

        public void Configure(AquariumDefinition aquariumDefinition, AquariumInhabitantProvider provider,
            FeedingUIController feeding, AquariumDetailsPanel details, GameObject hud, Button infoButton,
            Text aquariumName, Text volume, Text temperature, Text fishCount, Text status, Image badge)
        {
            definition = aquariumDefinition;
            inhabitants = provider;
            feedingUi = feeding;
            detailsPanel = details;
            compactHud = hud;
            detailsButton = infoButton;
            nameLabel = aquariumName;
            volumeLabel = volume;
            temperatureLabel = temperature;
            fishCountLabel = fishCount;
            statusLabel = status;
            statusBadge = badge;
        }

        private void OnEnable()
        {
            detailsButton?.onClick.AddListener(OpenDetails);
            if (inhabitants != null) inhabitants.PopulationChanged += HandlePopulationChanged;
            if (runtimeState.IsInitialized) runtimeState.Changed += Refresh;
            SetAquariumFocused(false);
        }

        private void Start()
        {
            inhabitants?.Refresh();
            if (!runtimeState.IsInitialized && definition != null)
            {
                runtimeState.Initialize("starter-aquarium-instance", definition, inhabitants?.TotalCount ?? 0);
                runtimeState.Changed += Refresh;
            }
            Refresh();
        }

        private void OnDisable()
        {
            detailsButton?.onClick.RemoveListener(OpenDetails);
            if (inhabitants != null) inhabitants.PopulationChanged -= HandlePopulationChanged;
            runtimeState.Changed -= Refresh;
            detailsPanel?.CloseImmediate();
        }

        public void SetAquariumFocused(bool isFocused)
        {
            focused = isFocused;
            if (runtimeState.IsInitialized) runtimeState.SetFocused(isFocused);
            if (compactHud != null) compactHud.SetActive(isFocused);
            if (detailsButton != null) detailsButton.interactable = isFocused;
            if (!isFocused) detailsPanel?.CloseImmediate();
            if (isFocused) Refresh();
        }

        public void SetInteractionEnabled(bool enabledState)
        {
            if (detailsButton != null) detailsButton.interactable = focused && enabledState;
            if (!enabledState) detailsPanel?.CloseImmediate();
        }

        public void OpenDetails()
        {
            if (!focused || detailsPanel == null || !runtimeState.IsInitialized) return;
            feedingUi?.CancelFeedingMode();
            detailsPanel.Show(BuildViewModel());
        }

        public void SetTemperature(float value) => runtimeState.SetTemperature(value);

        private void HandlePopulationChanged()
        {
            if (runtimeState.IsInitialized) runtimeState.SetFishCount(inhabitants.TotalCount);
            Refresh();
        }

        private AquariumViewModel BuildViewModel() =>
            new(definition, runtimeState, inhabitants != null ? inhabitants.Inhabitants : null);

        private void Refresh()
        {
            if (!runtimeState.IsInitialized || definition == null) return;
            var model = BuildViewModel();
            nameLabel.text = model.DisplayName;
            volumeLabel.text = $"◆ {model.VolumeText}";
            temperatureLabel.text = $"▲ {model.TemperatureText}";
            fishCountLabel.text = $"● {model.FishCountText}";
            statusLabel.text = model.StatusLabel;
            if (statusBadge != null) statusBadge.color = StatusColor(model.Status.Status);
        }

        private static Color StatusColor(AquariumStatus status) => status switch
        {
            AquariumStatus.Excellent => new Color(0.3f, 0.86f, 0.63f),
            AquariumStatus.Good => new Color(0.48f, 0.76f, 0.86f),
            AquariumStatus.Attention => new Color(0.96f, 0.72f, 0.3f),
            AquariumStatus.Critical => new Color(0.92f, 0.48f, 0.42f),
            _ => Color.gray
        };
    }
}
