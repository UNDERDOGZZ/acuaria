using System;
using System.Collections.Generic;
using Acuaria.Fish;
using UnityEngine;

namespace Acuaria.Aquarium.Decorations
{
    public enum DecorationCategory { Plant, Rock, Wood, Cave, Substrate, Artificial, OpenArea }

    [Serializable]
    public readonly struct HabitatContribution
    {
        public readonly float PlantCoverage, HidingPlaces, OpenSpaceConsumed, FlowResistance, LightingCoverage, VisualComplexity;
        public HabitatContribution(float plants, float hiding, float consumed, float flow, float lighting, float complexity)
        {
            PlantCoverage = Mathf.Clamp01(Safe(plants));
            HidingPlaces = Mathf.Max(0, Safe(hiding));
            OpenSpaceConsumed = Mathf.Clamp01(Safe(consumed));
            FlowResistance = Mathf.Clamp01(Safe(flow));
            LightingCoverage = Mathf.Clamp01(Safe(lighting));
            VisualComplexity = Mathf.Clamp01(Safe(complexity));
        }
        static float Safe(float value) => float.IsFinite(value) ? value : 0;
    }

    [CreateAssetMenu(menuName = "Acuaria/Aquarium/Decoration", fileName = "Decoration")]
    public sealed class DecorationDefinition : ScriptableObject
    {
        [SerializeField] string decorationId, displayName;
        [SerializeField, TextArea] string description;
        [SerializeField] DecorationCategory category;
        [SerializeField] Sprite sprite;
        [SerializeField] GameObject prefab;
        [SerializeField] Vector2 scale = Vector2.one;
        [SerializeField, Min(0)] float placeholderCost, occupiedSpace;
        [SerializeField, Range(0, 1)] float plantCoverage, openSpaceConsumed, flowResistance, lightingCoverage, visualComplexity;
        [SerializeField, Min(0)] float hidingPlaces;
        [SerializeField] FishSpeciesDefinition[] favouredSpecies = Array.Empty<FishSpeciesDefinition>();
        [SerializeField, TextArea] string educationalText;

        public string DecorationId => decorationId;
        public string DisplayName => displayName;
        public string Description => description;
        public DecorationCategory Category => category;
        public Sprite Sprite => sprite;
        public GameObject Prefab => prefab;
        public Vector2 Scale => scale;
        public float PlaceholderCost => placeholderCost;
        public float OccupiedSpace => occupiedSpace;
        public HabitatContribution Contribution => new(plantCoverage, hidingPlaces, openSpaceConsumed, flowResistance, lightingCoverage, visualComplexity);
        public IReadOnlyList<FishSpeciesDefinition> FavouredSpecies => favouredSpecies;
        public string EducationalText => educationalText;
        public bool IsValid => !string.IsNullOrWhiteSpace(decorationId) && !string.IsNullOrWhiteSpace(displayName) &&
            scale.x > 0 && scale.y > 0 && occupiedSpace >= 0;

        public void Configure(string id, string label, string summary, DecorationCategory group, Vector2 visualScale,
            float space, HabitatContribution habitat, string education, params FishSpeciesDefinition[] favoured)
        {
            decorationId = id?.Trim();
            displayName = label?.Trim();
            description = summary;
            category = group;
            scale = new Vector2(Mathf.Max(.05f, visualScale.x), Mathf.Max(.05f, visualScale.y));
            occupiedSpace = Mathf.Max(0, space);
            plantCoverage = habitat.PlantCoverage;
            hidingPlaces = habitat.HidingPlaces;
            openSpaceConsumed = habitat.OpenSpaceConsumed;
            flowResistance = habitat.FlowResistance;
            lightingCoverage = habitat.LightingCoverage;
            visualComplexity = habitat.VisualComplexity;
            educationalText = education;
            favouredSpecies = favoured ?? Array.Empty<FishSpeciesDefinition>();
        }
        public void ConfigureVisual(Sprite visualSprite, GameObject visualPrefab=null){sprite=visualSprite;prefab=visualPrefab;}
    }
}
