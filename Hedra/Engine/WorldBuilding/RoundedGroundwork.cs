using System;
using System.ComponentModel;
using Hedra.Engine.Generation;
using OpenTK;

namespace Hedra.Engine.WorldBuilding
{
    public class RoundedGroundwork : IGroundwork
    {
        public BlockType Type { get; set; } = BlockType.Path;
        public Vector3 Position { get; set; }
        public float Radius { get; set; }
        public float BonusHeight { get; set; }
        public bool IsPath => false;

        public RoundedGroundwork(Vector3 Position, float Radius)
        {
            this.Position = Position;
            this.Radius = Radius;
        }

        public RoundedGroundwork(Vector3 Position, float Radius, BlockType Type) : this(Position, Radius)
        {
            this.Type = Type;
        }

        public bool Affects(Vector2 Sample)
        {
            return (Sample - this.Position.Xz).LengthSquared < this.Radius * this.Radius;
        }
        
        public float Density(Vector2 Sample)
        {
            return 1 - Math.Min((Sample - this.Position.Xz).LengthSquared / (this.Radius * this.Radius), 1);
        }
    }
}
