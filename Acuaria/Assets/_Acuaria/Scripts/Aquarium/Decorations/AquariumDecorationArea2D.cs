using UnityEngine;

namespace Acuaria.Aquarium.Decorations
{
    public sealed class AquariumDecorationArea2D : MonoBehaviour
    {
        [SerializeField] Vector2 size = new(7.4f, 3.25f);
        [SerializeField] Vector2 center = new(0, -.08f);
        [SerializeField] bool showGizmos;
        public float MinX => center.x - Width * .5f;
        public float MaxX => center.x + Width * .5f;
        public float MinY => center.y - Height * .5f;
        public float MaxY => center.y + Height * .5f;
        public float BottomY => MinY;
        public float WaterTopY => MaxY;
        public Vector2 Center => center;
        public float Width => Mathf.Max(.1f, Mathf.Abs(size.x));
        public float Height => Mathf.Max(.1f, Mathf.Abs(size.y));
        public void Configure(Vector2 areaSize, Vector2 areaCenter)
        { size = new(Mathf.Max(.1f, Mathf.Abs(areaSize.x)), Mathf.Max(.1f, Mathf.Abs(areaSize.y))); center = areaCenter; }
        public Vector2 ToLocal(Vector2 normalized) => new(
            Mathf.Lerp(MinX, MaxX, Mathf.Clamp01(float.IsFinite(normalized.x) ? normalized.x : .5f)),
            Mathf.Lerp(MinY, MaxY, Mathf.Clamp01(float.IsFinite(normalized.y) ? normalized.y : .1f)));
        public Vector2 ToNormalized(Vector2 local) => new(
            Mathf.InverseLerp(MinX,MaxX,float.IsFinite(local.x)?local.x:center.x),
            Mathf.InverseLerp(MinY,MaxY,float.IsFinite(local.y)?local.y:center.y));
        public Vector2 WorldToNormalized(Vector2 world)=>ToNormalized(transform.InverseTransformPoint(world));
        public Vector2 NormalizedToWorld(Vector2 normalized)=>transform.TransformPoint(ToLocal(normalized));
        public bool Contains(Vector2 point) => point.x >= MinX && point.x <= MaxX && point.y >= MinY && point.y <= MaxY;
        void OnDrawGizmosSelected()
        { if(!showGizmos)return;Gizmos.matrix=transform.localToWorldMatrix;Gizmos.color=Color.yellow;Gizmos.DrawWireCube(center,size); }
    }
}
