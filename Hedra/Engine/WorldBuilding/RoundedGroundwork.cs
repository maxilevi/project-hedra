using System;
using System.Numerics;
using Hedra.Engine.Generation;
using Hedra.Numerics;

namespace Hedra.Engine.WorldBuilding
{
    public class RoundedGroundwork : BaseGroundwork
    {
        public RoundedGroundwork(Vector3 Position, float Radius, BlockType Type)
        {
            this.Position = Position;
            this.Radius = Radius;
            this.Type = Type;
        }

        private Vector3 Position { get; }
        private float Radius { get; }

        public override bool Affects(Vector2 Sample)
        {
            return (Sample - Position.Xz()).LengthSquared() < Radius * Radius;
        }

        public override float Density(Vector2 Sample)
        {
            return 1 - Math.Min((Sample - Position.Xz()).LengthSquared() / (Radius * Radius), 1);
        }

        public override BoundingBox ToBoundingBox()
        {
            return new BoundingBox(Position.Xz(), Radius * 2);
        }
    }
}