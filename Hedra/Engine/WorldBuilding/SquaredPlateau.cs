using System;
using System.Drawing;
using Hedra.Core;
using System.Numerics;

namespace Hedra.Engine.WorldBuilding
{
    public class SquaredPlateau : BasePlateau, IBoundingBox
    {
        private float Width { get; }
        public float Hardness { get; set; } = 1f;
        
        public SquaredPlateau(Vector2 Position, float Width) : base(Position)
        {
            this.Width = Width;
        }

        public override bool Collides(Vector2 Point)
        {
            return DoCollides(Point, Width);
        }

        private bool DoCollides(Vector2 Point, float Width)
        {
            var unCenteredPosition = Position - Width * Vector2.One * .5f;
            return Point.X < unCenteredPosition.X + Width && Point.X > unCenteredPosition.X &&
                   Point.Y < unCenteredPosition.Y + Width && Point.Y > unCenteredPosition.Y;
        }

        public override float Density(Vector2 Point)
        {
            if (Collides(Point)) return 1;
            if (!DoCollides(Point, Width * BorderMultiplier)) return 0;
            var halfWidth = Width * .5f;
            var nearest = new Vector2(
                Mathf.Clamp(Point.X, Position.X - halfWidth, Position.X + halfWidth),
                Mathf.Clamp(Point.Y, Position.Y - halfWidth, Position.Y + halfWidth)
            );
            return Math.Max(0f, 1.0f - (nearest - Point).LengthFast() * .04f * Hardness);
        }
        
        public Vector2 LeftCorner => Position - new Vector2(Width * .5f, 0);
        public Vector2 RightCorner => Position + new Vector2(Width * .5f, 0);
        public Vector2 BackCorner => Position - new Vector2(0, Width * .5f);
        public Vector2 FrontCorner => Position + new Vector2(0, Width * .5f);

        public override BoundingBox ToBoundingBox()
        {
            return new BoundingBox(Position, Width * BorderMultiplier);
        }

        public override BasePlateau Clone()
        {
            return new RoundedPlateau(Position, Width)
            {
                Hardness = Hardness,
                NoPlants = NoPlants,
                NoTrees = NoTrees
            };
        }
    }
}