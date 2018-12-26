using System;
using Hedra.Core;
using OpenTK;

namespace Hedra.Engine.WorldBuilding
{
    public struct BoundingBox : IBoundingBox
    {
        private readonly Vector2 _position;
        public float Width { get; }
        
        public BoundingBox(Vector2 Position, float Width)
        {
            _position = Position;
            this.Width = Width;
        }

        public bool Collides(Vector2 Point)
        {
            var unCenteredPosition = _position - Width * Vector2.One * .5f;
            return Point.X < unCenteredPosition.X + Width && Point.X > unCenteredPosition.X &&
                   Point.Y < unCenteredPosition.Y + Width && Point.Y > unCenteredPosition.Y;
        }

        public Vector2 LeftCorner => _position - new Vector2(Width * .5f, 0);
        public Vector2 RightCorner => _position + new Vector2(Width * .5f, 0);
        public Vector2 BackCorner => _position - new Vector2(0, Width * .5f);
        public Vector2 FrontCorner => _position + new Vector2(0, Width * .5f);
    }
}