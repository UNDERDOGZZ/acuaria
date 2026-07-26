using UnityEngine;

namespace Acuaria.Fish
{
    public sealed class AquariumSwimArea2D : MonoBehaviour
    {
        [SerializeField] private Vector2 size = new(7.65f, 3.55f);
        [SerializeField, Min(0f)] private float leftMargin = 0.2f;
        [SerializeField, Min(0f)] private float rightMargin = 0.2f;
        [SerializeField, Min(0f)] private float topMargin = 0.2f;
        [SerializeField, Min(0f)] private float bottomMargin = 0.25f;
        [SerializeField, Min(0f)] private float horizontalBoundaryPadding = 0.16f;
        [SerializeField, Min(0f)] private float verticalBoundaryPadding = 0.1f;
        [SerializeField] private bool showGizmos = true;

        public SwimBounds2D LocalBounds => new(
            -size.x * 0.5f + leftMargin,
            size.x * 0.5f - rightMargin,
            -size.y * 0.5f + bottomMargin,
            size.y * 0.5f - topMargin);

        public SwimBounds2D NavigationBounds => LocalBounds.Inset(horizontalBoundaryPadding, verticalBoundaryPadding);
        public Vector2 ClampLocal(Vector2 position) => NavigationBounds.Clamp(position);
        public Vector2 LocalToWorld(Vector2 position) => transform.TransformPoint(position);
        public Vector2 WorldToLocal(Vector2 position) => transform.InverseTransformPoint(position);

        public void Configure(Vector2 areaSize, Vector4 margins)
        {
            size = new Vector2(Mathf.Max(0.1f, areaSize.x), Mathf.Max(0.1f, areaSize.y));
            leftMargin = Mathf.Max(0f, margins.x);
            rightMargin = Mathf.Max(0f, margins.y);
            topMargin = Mathf.Max(0f, margins.z);
            bottomMargin = Mathf.Max(0f, margins.w);
        }

        private void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(0.2f, 0.9f, 0.8f, 0.8f);
            var bounds = NavigationBounds;
            Gizmos.DrawWireCube(new Vector3((bounds.Left + bounds.Right) * 0.5f, (bounds.Bottom + bounds.Top) * 0.5f),
                new Vector3(bounds.Right - bounds.Left, bounds.Top - bounds.Bottom, 0f));
        }
    }
}
