using UnityEngine;
using Acuaria.Fish.Care;
using Acuaria.Fish.Compatibility;

namespace Acuaria.Fish
{
    [CreateAssetMenu(menuName = "Acuaria/Fish/Species Definition", fileName = "FishSpecies")]
    public sealed class FishSpeciesDefinition : ScriptableObject
    {
        [SerializeField] private string speciesId;
        [SerializeField] private string displayName;
        [SerializeField, Min(0.01f)] private float minimumSpeed = 0.45f;
        [SerializeField, Min(0.01f)] private float maximumSpeed = 0.8f;
        [SerializeField, Min(0.01f)] private float minimumScale = 0.75f;
        [SerializeField, Min(0.01f)] private float maximumScale = 1f;
        [SerializeField, Min(0.1f)] private float minimumTargetDuration = 2.5f;
        [SerializeField, Min(0.1f)] private float maximumTargetDuration = 5f;
        [SerializeField, Range(-1f, 1f)] private float verticalPreference;
        [SerializeField] private Color prototypeColor = Color.cyan;
        [SerializeField] private SwimmingLevel swimmingLevel = SwimmingLevel.Any;
        [SerializeField] private Sprite prototypeSprite;
        [SerializeField] private FishCareRequirements care = new();
        [SerializeField] private FishSocialRequirements social = new();
        [SerializeField] private FishCompatibilityProfile compatibility = new();

        public string SpeciesId => speciesId;
        public string DisplayName => displayName;
        public float MinimumSpeed => minimumSpeed;
        public float MaximumSpeed => maximumSpeed;
        public float MinimumScale => minimumScale;
        public float MaximumScale => maximumScale;
        public float MinimumTargetDuration => minimumTargetDuration;
        public float MaximumTargetDuration => maximumTargetDuration;
        public float VerticalPreference => verticalPreference;
        public Color PrototypeColor => prototypeColor;
        public SwimmingLevel SwimmingLevel => swimmingLevel;
        public Sprite PrototypeSprite => prototypeSprite;
        public FishCareRequirements Care => care;
        public FishSocialRequirements Social => social;
        public FishCompatibilityProfile Compatibility => compatibility;
        public bool IsValid => !string.IsNullOrWhiteSpace(speciesId) && minimumSpeed > 0f &&
                               maximumSpeed >= minimumSpeed && minimumScale > 0f &&
                               maximumScale >= minimumScale && minimumTargetDuration > 0f &&
                               maximumTargetDuration >= minimumTargetDuration;

        public void ConfigureCare(FishCareRequirements careRequirements,FishSocialRequirements socialRequirements,
            FishCompatibilityProfile compatibilityProfile)
        {care=careRequirements??new FishCareRequirements();social=socialRequirements??new FishSocialRequirements();
         compatibility=compatibilityProfile??new FishCompatibilityProfile();}

        public void Configure(string id, string label, Vector2 speed, Vector2 scale, Vector2 targetDuration,
            float preference, Color color, SwimmingLevel level)
        {
            speciesId = id;
            displayName = label;
            minimumSpeed = Mathf.Max(0.01f, speed.x);
            maximumSpeed = Mathf.Max(minimumSpeed, speed.y);
            minimumScale = Mathf.Max(0.01f, scale.x);
            maximumScale = Mathf.Max(minimumScale, scale.y);
            minimumTargetDuration = Mathf.Max(0.1f, targetDuration.x);
            maximumTargetDuration = Mathf.Max(minimumTargetDuration, targetDuration.y);
            verticalPreference = Mathf.Clamp(preference, -1f, 1f);
            prototypeColor = color;
            swimmingLevel = level;
        }

        private void OnValidate()
        {
            minimumSpeed = Mathf.Max(0.01f, minimumSpeed);
            maximumSpeed = Mathf.Max(minimumSpeed, maximumSpeed);
            minimumScale = Mathf.Max(0.01f, minimumScale);
            maximumScale = Mathf.Max(minimumScale, maximumScale);
            minimumTargetDuration = Mathf.Max(0.1f, minimumTargetDuration);
            maximumTargetDuration = Mathf.Max(minimumTargetDuration, maximumTargetDuration);
        }
    }
}
