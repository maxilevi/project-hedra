using System;
using System.Numerics;

namespace Hedra.Engine.Generation
{
    public sealed class HighlightedArea : IDisposable
    {
        public Vector3 Position { get; set; }
        public Vector4 Color { get; set; }
        public float Radius { get; set; }
        public bool Stop { get; private set; }
        public bool OnlyWater { get; set; }

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

        public void Reset()
        {
            Radius = 0;
            Color = Vector4.Zero;
            Position = Vector3.Zero;
            OnlyWater = false;
        }

        public void Copy(HighlightedArea Area)
        {
            Position = Area.Position;
            Radius = Area.Radius;
            Color = Area.Color;
            OnlyWater = Area.OnlyWater;
        }
        
        public void Dispose()
        {
            Stop = true;
        }
    }

    public sealed class HighlightedAreaWrapper : IDisposable
    {
        private HighlightedArea _area;

        public HighlightedArea Area
        {
            get => _area;
            set
            {
                _area = value;
                if(_area != null)
                    _area.OnlyWater = OnlyWater;
            }
        }

        public void Dispose()
        {
            Area?.Dispose();
        }
        
        public bool OnlyWater { get; set; }
    }
}
