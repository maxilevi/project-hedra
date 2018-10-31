using System;
using OpenTK;

namespace Hedra.Engine.Generation
{
    public sealed class HighlightedArea : IDisposable
    {
        public Vector3 Position { get; set; }
        public Vector4 Color { get; set; }
        public float Radius { get; set; }
        public bool Stop { get; private set; }

        public HighlightedArea()
        {           
        }
        
        public HighlightedArea(Vector3 Position, Vector4 Color, float Radius)
        {
            this.Position = Position;
            this.Color = Color;
            this.Radius = Radius;
        }

        public Vector4 AreaPosition => new Vector4(Position.X, Position.Y, Position.Z, Radius);

        public Vector4 AreaColor => new Vector4(Color.X, Color.Y, Color.Z, Color.W);

        public bool IsEmpty => Position == Vector3.Zero && Color == Vector4.Zero && Math.Abs(Radius) < 0.0005f;

        public void Dispose()
        {
            Stop = true;
        }
    }

    public sealed class HighlightedAreaWrapper : IDisposable
    {
        public HighlightedArea Area { get; set; }

        public void Dispose()
        {
            Area?.Dispose();
        }
    }
}
