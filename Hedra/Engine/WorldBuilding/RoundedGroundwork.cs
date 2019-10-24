using System;
using System.ComponentModel;
using Hedra.Engine.Generation;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.Engine.WorldBuilding
{
    public class RoundedGroundwork : BaseGroundwork
    {
        private Vector3 Position { get; set; }
        private float Radius { get; set; }

        public RoundedGroundwork(Vector3 Position, float Radius, BlockType Type)
        {
            this.Position = Position;
            this.Radius = Radius;
            this.Type = Type;
        }

        public override bool Affects(Vector2 Sample)
        {
            return (Sample - this.Position.Xz()).LengthSquared() < this.Radius * this.Radius;
        }
        
        public override float Density(Vector2 Sample)
        {
            return 1 - Math.Min((Sample - this.Position.Xz()).LengthSquared() / (this.Radius * this.Radius), 1);
        }

        public override BoundingBox ToBoundingBox()
        {
            return new BoundingBox(Position.Xz(), Radius * 2);
        }
    }
}
