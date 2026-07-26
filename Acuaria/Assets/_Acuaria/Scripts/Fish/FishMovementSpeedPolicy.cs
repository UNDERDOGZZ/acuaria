using UnityEngine;
namespace Acuaria.Fish
{
    public static class FishMovementSpeedPolicy
    {
        public const float MinimumActiveMultiplier=0.35f;
        public const float MaximumMultiplier=2f;
        public static float Calculate(float baseSpeed,float welfare,float behaviour,float maintenance,bool explicitlyPaused)
        {
            if(explicitlyPaused)return 0f;
            var safeBase=Safe(baseSpeed,0f);
            var combined=Safe(welfare,1f)*Safe(behaviour,1f)*Safe(maintenance,1f);
            combined=Mathf.Clamp(combined,MinimumActiveMultiplier,MaximumMultiplier);
            var result=safeBase*combined;
            return float.IsFinite(result)&&result>=0f?result:0f;
        }
        static float Safe(float value,float fallback)=>float.IsFinite(value)&&value>=0f?value:fallback;
    }
}
