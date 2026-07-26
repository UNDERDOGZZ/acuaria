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
        public float Width => Right - Left;
        public float Height => Top - Bottom;
        public Vector2 Center => new((Left + Right) * 0.5f, (Bottom + Top) * 0.5f);
        public bool IsValid => Width > Mathf.Epsilon && Height > Mathf.Epsilon;
        public bool Contains(Vector2 point) => point.x >= Left && point.x <= Right && point.y >= Bottom && point.y <= Top;
        public Vector2 Clamp(Vector2 point) => new(Mathf.Clamp(point.x, Left, Right), Mathf.Clamp(point.y, Bottom, Top));

        public SwimBounds2D Inset(float horizontalPadding, float verticalPadding)
        {
            var safeHorizontal = Mathf.Clamp(SafePadding(horizontalPadding), 0f, Width * 0.49f);
            var safeVertical = Mathf.Clamp(SafePadding(verticalPadding), 0f, Height * 0.49f);
            return new SwimBounds2D(Left + safeHorizontal, Right - safeHorizontal,
                Bottom + safeVertical, Top - safeVertical);
        }

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

        private static float SafePadding(float value) => float.IsFinite(value) ? Mathf.Max(0f, value) : 0f;
    }
}
