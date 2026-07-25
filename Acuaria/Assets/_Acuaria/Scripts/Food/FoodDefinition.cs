using UnityEngine;

namespace Acuaria.Food
{
    [CreateAssetMenu(menuName = "Acuaria/Food/Definition", fileName = "FoodDefinition")]
    public sealed class FoodDefinition : ScriptableObject
    {
        [SerializeField] private string foodId;
        [SerializeField] private string displayName;
        [SerializeField, Min(0.01f)] private float fallSpeed = 0.22f;
        [SerializeField, Min(0.1f)] private float maximumLifetime = 18f;
        [SerializeField, Min(0.01f)] private float detectionRadius = 4f;
        [SerializeField, Min(0.01f)] private float consumptionRadius = 0.3f;
        [SerializeField, Min(0.01f)] private float minimumVisualSize = 0.08f;
        [SerializeField, Min(0.01f)] private float maximumVisualSize = 0.14f;
        [SerializeField] private Color prototypeColor = new(0.95f, 0.72f, 0.26f);
        [SerializeField, Min(0f)] private float nutrition = 0.22f;
        [SerializeField] private FoodTargetZone targetZone = FoodTargetZone.Surface;

        public string FoodId => foodId;
        public string DisplayName => displayName;
        public float FallSpeed => fallSpeed;
        public float MaximumLifetime => maximumLifetime;
        public float DetectionRadius => detectionRadius;
        public float ConsumptionRadius => consumptionRadius;
        public float MinimumVisualSize => minimumVisualSize;
        public float MaximumVisualSize => maximumVisualSize;
        public Color PrototypeColor => prototypeColor;
        public float Nutrition => nutrition;
        public FoodTargetZone TargetZone => targetZone;
        public bool IsValid => !string.IsNullOrWhiteSpace(foodId) && fallSpeed > 0f && maximumLifetime > 0f &&
                               detectionRadius > 0f && consumptionRadius > 0f && minimumVisualSize > 0f &&
                               maximumVisualSize >= minimumVisualSize && nutrition >= 0f;

        public void Configure(string id, string label, float speed, float lifetime, float detection,
            float consumption, Vector2 visualSize, Color color, float nutritionValue, FoodTargetZone zone)
        {
            foodId = id;
            displayName = label;
            fallSpeed = Mathf.Max(0.01f, speed);
            maximumLifetime = Mathf.Max(0.1f, lifetime);
            detectionRadius = Mathf.Max(0.01f, detection);
            consumptionRadius = Mathf.Max(0.01f, consumption);
            minimumVisualSize = Mathf.Max(0.01f, visualSize.x);
            maximumVisualSize = Mathf.Max(minimumVisualSize, visualSize.y);
            prototypeColor = color;
            nutrition = Mathf.Max(0f, nutritionValue);
            targetZone = zone;
        }

        private void OnValidate() => Configure(foodId, displayName, fallSpeed, maximumLifetime, detectionRadius,
            consumptionRadius, new Vector2(minimumVisualSize, maximumVisualSize), prototypeColor, nutrition, targetZone);
    }
}
