using System;
using UnityEngine;

namespace Acuaria.Aquarium.Decorations
{
    public enum DecorationVisualLayer { Background, Substrate, Midground, Foreground }
    public enum DecorationRenderStatus { Visible, MissingPrefab, MissingSprite, InvalidPosition, InvalidScale, HiddenByConfiguration, SpawnFailed }

    [Serializable]
    public sealed class DecorationPlacementData
    {
        [SerializeField] string instanceId;
        [SerializeField] DecorationDefinition definition;
        [SerializeField] Vector2 normalizedPosition = new(.5f, .1f);
        [SerializeField] float localRotation;
        [SerializeField] Vector2 localScale = Vector2.one;
        [SerializeField] bool flipX;
        [SerializeField] int sortingOrderOffset;
        [SerializeField] bool isVisible = true, isEnabled = true;
        [SerializeField] DecorationVisualLayer visualLayer = DecorationVisualLayer.Midground;

        public string InstanceId => instanceId;
        public DecorationDefinition Definition => definition;
        public Vector2 NormalizedPosition => Safe01(normalizedPosition);
        public float LocalRotation => float.IsFinite(localRotation) ? localRotation : 0;
        public Vector2 LocalScale => SafeScale(localScale);
        public bool FlipX => flipX;
        public int SortingOrderOffset => sortingOrderOffset;
        public bool IsVisible => isVisible;
        public bool IsEnabled => isEnabled;
        public DecorationVisualLayer VisualLayer => visualLayer;
        public bool IsValid => !string.IsNullOrWhiteSpace(instanceId) && definition != null;

        public DecorationPlacementData(string id, DecorationDefinition value, Vector2 position,
            Vector2 scale, float rotation = 0, bool flip = false, int order = 0,
            DecorationVisualLayer layer = DecorationVisualLayer.Midground)
        {
            instanceId = id?.Trim(); definition = value; normalizedPosition = Safe01(position);
            localScale = SafeScale(scale); localRotation = float.IsFinite(rotation) ? rotation : 0;
            flipX = flip; sortingOrderOffset = order; visualLayer = layer;
            isVisible = true; isEnabled = true;
        }

        static Vector2 Safe01(Vector2 value) => new(
            float.IsFinite(value.x) ? Mathf.Clamp01(value.x) : .5f,
            float.IsFinite(value.y) ? Mathf.Clamp01(value.y) : .1f);
        static Vector2 SafeScale(Vector2 value) => new(
            Mathf.Clamp(float.IsFinite(value.x) ? Mathf.Abs(value.x) : 1, .1f, 5),
            Mathf.Clamp(float.IsFinite(value.y) ? Mathf.Abs(value.y) : 1, .1f, 5));
    }
}
