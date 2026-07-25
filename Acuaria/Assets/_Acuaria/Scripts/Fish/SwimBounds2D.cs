using UnityEngine;

namespace Acuaria.Fish
{
    public readonly struct SwimBounds2D
    {
        public SwimBounds2D(float left, float right, float bottom, float top)
        {
            Left = Mathf.Min(left, right);
            Right = Mathf.Max(left, right);
            Bottom = Mathf.Min(bottom, top);
            Top = Mathf.Max(bottom, top);
        }

        public float Left { get; }
        public float Right { get; }
        public float Bottom { get; }
        public float Top { get; }
        public bool Contains(Vector2 point) => point.x >= Left && point.x <= Right && point.y >= Bottom && point.y <= Top;
        public Vector2 Clamp(Vector2 point) => new(Mathf.Clamp(point.x, Left, Right), Mathf.Clamp(point.y, Bottom, Top));

        public SwimBounds2D ForLevel(SwimmingLevel level)
        {
            var height = Top - Bottom;
            return level switch
            {
                SwimmingLevel.Upper => new SwimBounds2D(Left, Right, Bottom + height * 0.62f, Top),
                SwimmingLevel.Middle => new SwimBounds2D(Left, Right, Bottom + height * 0.31f, Bottom + height * 0.69f),
                SwimmingLevel.Lower => new SwimBounds2D(Left, Right, Bottom, Bottom + height * 0.38f),
                _ => this
            };
        }
    }
}
