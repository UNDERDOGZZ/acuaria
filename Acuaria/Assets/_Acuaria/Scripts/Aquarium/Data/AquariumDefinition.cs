using UnityEngine;

namespace Acuaria.Aquarium
{
    [CreateAssetMenu(menuName = "Acuaria/Aquarium/Definition", fileName = "AquariumDefinition")]
    public sealed class AquariumDefinition : ScriptableObject
    {
        [SerializeField] private string aquariumId;
        [SerializeField] private string displayName;
        [SerializeField, Min(1f)] private float nominalVolumeLitres = 50f;
        [SerializeField] private float targetTemperatureMin = 24f;
        [SerializeField] private float targetTemperatureMax = 26f;
        [SerializeField] private float initialTemperature = 25f;
        [SerializeField, Min(0)] private int recommendedFishCapacity = 3;
        [SerializeField, TextArea] private string description;
        [SerializeField, TextArea] private string educationTip;
        [SerializeField] private Sprite icon;
        [SerializeField] private Color themeColor = new(0.18f, 0.72f, 0.78f);

        public string AquariumId => aquariumId;
        public string DisplayName => displayName;
        public float NominalVolumeLitres => nominalVolumeLitres;
        public float TargetTemperatureMin => targetTemperatureMin;
        public float TargetTemperatureMax => targetTemperatureMax;
        public float InitialTemperature => initialTemperature;
        public int RecommendedFishCapacity => recommendedFishCapacity;
        public string Description => description;
        public string EducationTip => educationTip;
        public Sprite Icon => icon;
        public Color ThemeColor => themeColor;
        public bool IsValid => !string.IsNullOrWhiteSpace(aquariumId) &&
                               !string.IsNullOrWhiteSpace(displayName) &&
                               nominalVolumeLitres > 0f &&
                               targetTemperatureMax >= targetTemperatureMin &&
                               initialTemperature is >= -10f and <= 50f &&
                               recommendedFishCapacity >= 0;

        public void Configure(string id, string label, float litres, Vector2 temperatureRange,
            float startingTemperature, int capacity, string summary, string tip, Color color)
        {
            aquariumId = id;
            displayName = label;
            nominalVolumeLitres = Mathf.Max(0.1f, litres);
            targetTemperatureMin = Mathf.Clamp(temperatureRange.x, -10f, 50f);
            targetTemperatureMax = Mathf.Clamp(Mathf.Max(targetTemperatureMin, temperatureRange.y), -10f, 50f);
            initialTemperature = Mathf.Clamp(startingTemperature, -10f, 50f);
            recommendedFishCapacity = Mathf.Max(0, capacity);
            description = summary;
            educationTip = tip;
            themeColor = color;
        }

        private void OnValidate()
        {
            nominalVolumeLitres = Mathf.Max(0.1f, nominalVolumeLitres);
            targetTemperatureMin = Mathf.Clamp(targetTemperatureMin, -10f, 50f);
            targetTemperatureMax = Mathf.Clamp(Mathf.Max(targetTemperatureMin, targetTemperatureMax), -10f, 50f);
            initialTemperature = Mathf.Clamp(initialTemperature, -10f, 50f);
            recommendedFishCapacity = Mathf.Max(0, recommendedFishCapacity);
        }
    }
}
