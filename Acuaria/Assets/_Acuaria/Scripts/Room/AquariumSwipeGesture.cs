using UnityEngine;

namespace Acuaria.Room
{
    public enum AquariumSwipeResult { None, Previous, Next }

    public static class AquariumSwipeGesture
    {
        public static AquariumSwipeResult Evaluate(Vector2 start,Vector2 end,float elapsed,
            float minimumDistance=90f,float horizontalDominance=1.35f,float maximumDuration=1.25f)
        {
            if(elapsed<0f||elapsed>maximumDuration)return AquariumSwipeResult.None;
            var delta=end-start;
            if(Mathf.Abs(delta.x)<minimumDistance||Mathf.Abs(delta.x)<Mathf.Abs(delta.y)*horizontalDominance)
                return AquariumSwipeResult.None;
            return delta.x<0f?AquariumSwipeResult.Next:AquariumSwipeResult.Previous;
        }
    }
}
