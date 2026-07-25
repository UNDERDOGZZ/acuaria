using UnityEngine;

namespace Acuaria.Food
{
    public sealed class AquariumFeedingArea2D : MonoBehaviour
    {
        [SerializeField] private Vector2 size = new(7.15f, 3.2f);
        [SerializeField, Min(0f)] private float lateralMargin = 0.25f;
        [SerializeField, Range(0.5f, 1f)] private float surfaceHeight = 0.9f;
        [SerializeField] private bool showGizmos = true;

        public float Left => -size.x * 0.5f + lateralMargin;
        public float Right => size.x * 0.5f - lateralMargin;
        public float Bottom => -size.y * 0.5f;
        public float Top => size.y * 0.5f;
        public float FeedingHeight => Mathf.Lerp(Bottom, Top, surfaceHeight);

        public void Configure(Vector2 areaSize, float margin, float normalizedHeight)
        {
            size = new Vector2(Mathf.Max(0.1f, areaSize.x), Mathf.Max(0.1f, areaSize.y));
            lateralMargin = Mathf.Max(0f, margin);
            surfaceHeight = Mathf.Clamp(normalizedHeight, 0.5f, 1f);
        }

        public bool ContainsWorldPoint(Vector2 worldPoint)
        {
            var local = (Vector2)transform.InverseTransformPoint(worldPoint);
            return local.x >= Left && local.x <= Right && local.y >= Bottom && local.y <= Top;
        }

        public Vector2 ProjectWorldToSurface(Vector2 worldPoint)
        {
            var local = (Vector2)transform.InverseTransformPoint(worldPoint);
            local.x = Mathf.Clamp(local.x, Left, Right);
            local.y = FeedingHeight;
            return transform.TransformPoint(local);
        }

        public Vector2 WorldToLocal(Vector2 worldPoint) => transform.InverseTransformPoint(worldPoint);

        private void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(1f, 0.72f, 0.2f, 0.8f);
            Gizmos.DrawLine(new Vector3(Left, FeedingHeight), new Vector3(Right, FeedingHeight));
        }
    }
}
