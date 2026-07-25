using System.Collections.Generic;
using Acuaria.Aquarium;
using Acuaria.Food;
using Acuaria.Simulation.Water;
using Acuaria.UI.Maintenance;
using Acuaria.Fish.Welfare;
using System;
using Acuaria.UI.Progression;
using TMPro;
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
        [SerializeField] private AquariumHUDResponsiveLayout responsiveLayout;
        [SerializeField] private GameObject compactHud;
        [SerializeField] private Button backButton;
        [SerializeField] private Button feedButton;
        [SerializeField] private Button detailsButton;
        [SerializeField] private TMP_Text aquariumNameText;
        [SerializeField] private TMP_Text volumeText;
        [SerializeField] private TMP_Text temperatureText;
        [SerializeField] private TMP_Text fishCountText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Image statusBadge;
        private readonly AquariumRuntimeState runtimeState = new();
        private bool focused;
        private WaterChemistryViewModel waterChemistry;
        private string welfareDetails;
        [SerializeField] private AquariumMaintenanceController maintenance;
        [SerializeField] private AquaristJournalController journal;

        public bool IsVisible => compactHud != null && compactHud.activeSelf;
        public bool AreDetailsOpen => detailsPanel != null && detailsPanel.IsOpen;
        public AquariumRuntimeState RuntimeState => runtimeState;
        public TMP_Text AquariumNameText => aquariumNameText;
        public TMP_Text VolumeText => volumeText;
        public TMP_Text TemperatureText => temperatureText;
        public TMP_Text FishCountText => fishCountText;
        public TMP_Text StatusText => statusText;
        public event Action DetailsOpened;

        public void Configure(AquariumDefinition aquariumDefinition, AquariumInhabitantProvider provider,
            FeedingUIController feeding, AquariumDetailsPanel details, AquariumHUDResponsiveLayout responsive,
            GameObject hud, Button back, Button feed, Button infoButton, TMP_Text aquariumName, TMP_Text volume,
            TMP_Text temperature, TMP_Text fishCount, TMP_Text status, Image badge)
        {
            definition = aquariumDefinition;
            inhabitants = provider;
            feedingUi = feeding;
            detailsPanel = details;
            responsiveLayout = responsive;
            compactHud = hud;
            backButton = back;
            feedButton = feed;
            detailsButton = infoButton;
            aquariumNameText = aquariumName;
            volumeText = volume;
            temperatureText = temperature;
            fishCountText = fishCount;
            statusText = status;
            statusBadge = badge;
        }

        private void Awake()
        {
            var issues = new List<string>(8);
            CollectReferenceIssues(issues);
            for (var index = 0; index < issues.Count; index++)
                Debug.LogError($"AquariumHUD: {issues[index]}", this);
        }

        private void OnEnable()
        {
            detailsButton?.onClick.AddListener(OpenDetails);
            if (inhabitants != null) inhabitants.PopulationChanged += HandlePopulationChanged;
            if (runtimeState.IsInitialized) runtimeState.Changed += Refresh;
            responsiveLayout?.Refresh();
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
            maintenance?.SetAquariumFocused(isFocused);
            journal?.SetAquariumFocused(isFocused);
            if (runtimeState.IsInitialized) runtimeState.SetFocused(isFocused);
            if (compactHud != null) compactHud.SetActive(isFocused);
            if (detailsButton != null) detailsButton.interactable = isFocused;
            if (!isFocused) detailsPanel?.CloseImmediate();
            if (isFocused)
            {
                responsiveLayout?.Refresh();
                Refresh();
            }
        }

        public void SetMaintenanceController(AquariumMaintenanceController controller) => maintenance = controller;
        public void SetJournalController(AquaristJournalController controller) => journal = controller;

        public void SetInteractionEnabled(bool enabledState)
        {
            if (detailsButton != null) detailsButton.interactable = focused && enabledState;
            if (!enabledState) detailsPanel?.CloseImmediate();
        }

        public void OpenDetails()
        {
            if (!focused || detailsPanel == null || !runtimeState.IsInitialized) return;
            feedingUi?.CancelFeedingMode();
            detailsPanel.SetWaterChemistry(waterChemistry);
            detailsPanel.SetFishWelfare(welfareDetails);
            detailsPanel.Show(BuildViewModel());
            DetailsOpened?.Invoke();
        }

        public void SetFishWelfare(string compact,string details,FishWelfareStatus status)
        {
            welfareDetails=details;
            if(statusText!=null&&waterChemistry!=null)statusText.text=$"Agua: {waterChemistry.QualityLabel} · {compact}";
            if(detailsPanel!=null&&detailsPanel.IsOpen)detailsPanel.SetFishWelfare(details);
        }

        public void SetWaterChemistry(WaterChemistryViewModel chemistry)
        {
            waterChemistry = chemistry;
            if (statusText != null && chemistry != null) statusText.text = $"Agua: {chemistry.QualityLabel}";
            if (statusBadge != null && chemistry != null) statusBadge.color = WaterQualityColor(chemistry.Quality.Status);
            if (detailsPanel != null && detailsPanel.IsOpen) detailsPanel.SetWaterChemistry(chemistry);
        }

        public void SetTemperature(float value) => runtimeState.SetTemperature(value);

        public void CollectReferenceIssues(List<string> issues)
        {
            if (issues == null) return;
            CheckMissing(issues, definition, nameof(definition));
            CheckMissing(issues, compactHud, nameof(compactHud));
            CheckMissing(issues, aquariumNameText, nameof(aquariumNameText));
            CheckMissing(issues, volumeText, nameof(volumeText));
            CheckMissing(issues, temperatureText, nameof(temperatureText));
            CheckMissing(issues, fishCountText, nameof(fishCountText));
            CheckMissing(issues, statusText, nameof(statusText));
            CheckMissing(issues, detailsButton, nameof(detailsButton));
            CheckMissing(issues, feedButton, nameof(feedButton));
            CheckMissing(issues, backButton, nameof(backButton));

            CheckDistinct(issues, aquariumNameText, volumeText, "AquariumNameText", "VolumeText");
            CheckDistinct(issues, temperatureText, fishCountText, "TemperatureText", "FishCountText");
            CheckDistinct(issues, statusText, temperatureText, "StatusText", "TemperatureText");
            CheckDistinct(issues, statusText, fishCountText, "StatusText", "FishCountText");
        }

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
            aquariumNameText.text = model.DisplayName;
            volumeText.text = model.VolumeText;
            temperatureText.text = model.TemperatureText;
            fishCountText.text = model.FishCountText;
            if (waterChemistry == null)
            {
                statusText.text = model.StatusLabel;
                if (statusBadge != null) statusBadge.color = StatusColor(model.Status.Status);
            }
        }

        private static void CheckMissing(List<string> issues, UnityEngine.Object value, string fieldName)
        {
            if (value == null) issues.Add($"falta la referencia '{fieldName}'.");
        }

        private static void CheckDistinct(List<string> issues, TMP_Text first, TMP_Text second,
            string firstName, string secondName)
        {
            if (first != null && first == second)
                issues.Add($"'{firstName}' y '{secondName}' apuntan al mismo texto.");
        }

        private static Color StatusColor(AquariumStatus status) => status switch
        {
            AquariumStatus.Excellent => new Color(0.3f, 0.86f, 0.63f),
            AquariumStatus.Good => new Color(0.48f, 0.76f, 0.86f),
            AquariumStatus.Attention => new Color(0.96f, 0.72f, 0.3f),
            AquariumStatus.Critical => new Color(0.92f, 0.48f, 0.42f),
            _ => Color.gray
        };

        private static Color WaterQualityColor(WaterQualityStatus status) => status switch
        {
            WaterQualityStatus.Excellent => new Color(0.3f, 0.86f, 0.63f),
            WaterQualityStatus.Good => new Color(0.48f, 0.76f, 0.86f),
            WaterQualityStatus.Warning => new Color(0.96f, 0.72f, 0.3f),
            _ => new Color(0.92f, 0.48f, 0.42f)
        };
    }
}
