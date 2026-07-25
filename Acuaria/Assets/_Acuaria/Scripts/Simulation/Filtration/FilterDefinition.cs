using UnityEngine;

namespace Acuaria.Simulation.Filtration
{
    public enum FilterType { Internal, HangOnBack, Canister, Sponge }
    [CreateAssetMenu(menuName = "Acuaria/Filter Definition")]
    public sealed class FilterDefinition : ScriptableObject
    {
        [SerializeField] private string filterId = "starter-internal-filter";
        [SerializeField] private string displayName = "Filtro interno inicial";
        [SerializeField] private Vector2 recommendedVolume = new(30f, 70f);
        [SerializeField, Range(0f, 1f)] private float baseEfficiency = 0.85f;
        [SerializeField, Min(0f)] private float additionalBiologicalCapacity = 0.35f;
        [SerializeField, Min(0f)] private float dirtAccumulationPerHour = 0.002f;
        [SerializeField, Range(0f, 1f)] private float maximumDirtPenalty = 0.7f;
        [SerializeField, Min(0.01f)] private float recommendedMaintenanceHours = 168f;
        [SerializeField, Range(0f, 1f)] private float initialDirt = 0.08f;
        [SerializeField] private FilterType filterType = FilterType.Internal;
        [TextArea, SerializeField] private string education = "El material biológico alberga bacterias beneficiosas.";
        public string FilterId => filterId; public string DisplayName => displayName;
        public Vector2 RecommendedVolume => recommendedVolume; public float BaseEfficiency => baseEfficiency;
        public float AdditionalBiologicalCapacity => additionalBiologicalCapacity;
        public float DirtAccumulationPerHour => dirtAccumulationPerHour; public float MaximumDirtPenalty => maximumDirtPenalty;
        public float RecommendedMaintenanceHours => recommendedMaintenanceHours; public float InitialDirt => initialDirt;
        public FilterType FilterType => filterType; public string Education => education;
        public void Configure(string id, string label, Vector2 volume, float efficiency, float capacity,
            float dirtRate, float dirtPenalty, float frequency, float dirt, FilterType type, string text)
        {
            filterId=id?.Trim(); displayName=label; recommendedVolume=new Vector2(Safe(volume.x),Safe(volume.y));
            baseEfficiency=Mathf.Clamp01(Safe(efficiency)); additionalBiologicalCapacity=Safe(capacity);
            dirtAccumulationPerHour=Safe(dirtRate); maximumDirtPenalty=Mathf.Clamp01(Safe(dirtPenalty));
            recommendedMaintenanceHours=Mathf.Max(0.01f,Safe(frequency)); initialDirt=Mathf.Clamp01(Safe(dirt));
            filterType=type; education=text;
        }
        public bool IsValid => !string.IsNullOrWhiteSpace(filterId) && recommendedVolume.x <= recommendedVolume.y &&
            recommendedMaintenanceHours > 0f;
        private static float Safe(float value)=>float.IsFinite(value)?Mathf.Max(0f,value):0f;
    }
}
