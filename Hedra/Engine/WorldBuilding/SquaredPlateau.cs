using System;
using System.Drawing;
using Hedra.Core;
using OpenTK;

namespace Hedra.Engine.WorldBuilding
{
    public class SquaredPlateau : BasePlateau
    {
        private float Width { get; }
        public float Hardness { get; set; } = 1.0f;
        
        public SquaredPlateau(Vector3 Position, float Width) : base(Position)
        {
            this.Width = Width;
        }

        public override bool Collides(Vector2 Point)
        {
            var unCenteredPosition = Position.Xz - Width * Vector2.One * .5f;
            return Point.X < unCenteredPosition.X + Width && Point.X > unCenteredPosition.X &&
                   Point.Y < unCenteredPosition.Y + Width && Point.Y > unCenteredPosition.Y;
        }

        public override float Density(Vector2 Point)
        {
            if (Collides(Point)) return 1;
            var halfWidth = Width * .5f;
            var nearest = new Vector2(
                Mathf.Clamp(Point.X, Position.X - halfWidth, Position.X + halfWidth),
                Mathf.Clamp(Point.Y, Position.Z - halfWidth, Position.Z + halfWidth)
            );
            return Math.Max(0f, 1.0f - (nearest - Point).LengthFast * .04f * Hardness);
        }
        
        public Vector2 LeftCorner => Position.Xz - new Vector2(Width * .5f, 0);
        public Vector2 RightCorner => Position.Xz + new Vector2(Width * .5f, 0);
        public Vector2 BackCorner => Position.Xz - new Vector2(0, Width * .5f);
        public Vector2 FrontCorner => Position.Xz + new Vector2(0, Width * .5f);
        
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