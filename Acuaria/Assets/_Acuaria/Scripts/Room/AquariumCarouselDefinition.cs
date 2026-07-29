using UnityEngine;

namespace Acuaria.Room
{
    [CreateAssetMenu(menuName="Acuaria/Room/Aquarium Carousel Definition")]
    public sealed class AquariumCarouselDefinition:ScriptableObject
    {
        [SerializeField,Min(.1f)] float aquariumSpacing=12f;
        [SerializeField,Range(.1f,.25f)] float sidePreviewPercentage=.16f;
        [SerializeField,Min(.1f)] float transitionDuration=.48f;
        [SerializeField,Min(0f)] float extraDurationPerSlot=.16f;
        [SerializeField,Min(.1f)] float maximumDuration=.8f;
        [SerializeField,Min(20f)] float swipeThresholdPixels=90f;
        [SerializeField,Min(1f)] float horizontalDominanceRatio=1.35f;
        [SerializeField,Min(.1f)] float maximumSwipeDuration=1.25f;
        [SerializeField] AnimationCurve transitionCurve=AnimationCurve.EaseInOut(0,0,1,1);
        public float Spacing=>Safe(aquariumSpacing,12f);
        public float SidePreview=>Mathf.Clamp(sidePreviewPercentage,.1f,.25f);
        public float SwipeThreshold=>Safe(swipeThresholdPixels,90f);
        public float HorizontalDominance=>Safe(horizontalDominanceRatio,1.35f);
        public float MaximumSwipeDuration=>Safe(maximumSwipeDuration,1.25f);
        public AnimationCurve Curve=>transitionCurve??AnimationCurve.EaseInOut(0,0,1,1);
        public float DurationForDistance(int slots)=>Mathf.Min(Safe(maximumDuration,.8f),
            Safe(transitionDuration,.48f)+Mathf.Max(0,slots-1)*Mathf.Max(0,extraDurationPerSlot));
        static float Safe(float value,float fallback)=>float.IsFinite(value)&&value>0?value:fallback;
        public void Configure(float spacing,float preview,float duration)
        {aquariumSpacing=spacing;sidePreviewPercentage=preview;transitionDuration=duration;}
    }
}
