using OpenTK;

namespace Hedra.Engine.Generation
{
    public class HighlightedArea
    {
        public Vector3 Position;
        public Vector4 Color;
        public float Radius;

        public HighlightedArea() { }
        public HighlightedArea(Vector3 Position, Vector4 Color, float Radius)
        {
            this.Position = Position;
            this.Color = Color;
            this.Radius = Radius;
        }

        public Vector4 AreaPosition => new Vector4(Position.X, Position.Y, Position.Z, Radius);

        public Vector4 AreaColor => new Vector4(Color.X, Color.Y, Color.Z, Color.W);

        public bool IsEmpty => Position == Vector3.Zero && Color == Vector4.Zero && Radius == 0;
    }
}
