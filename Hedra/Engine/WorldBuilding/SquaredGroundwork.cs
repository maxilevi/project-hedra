using System;
using Hedra.Engine.Generation;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.Engine.WorldBuilding
{
    public class SquaredGroundwork : BaseGroundwork
    {
        private Vector3 Position { get; }
        private float Width { get; }
        
        public SquaredGroundwork(Vector3 Position, float Width, BlockType Type)
        {
            this.Position = Position;
            this.Width = Width;
            this.Type = Type;
        }

        public override bool Affects(Vector2 Sample)
        {
            var uncenteredPosition = Position.Xz() - Width * Vector2.One * .5f;
            return Sample.X < uncenteredPosition.X + Width && Sample.X > uncenteredPosition.X &&
                   Sample.Y < uncenteredPosition.Y + Width && Sample.Y > uncenteredPosition.Y;
        }

        public override float Density(Vector2 Sample)
        {
            return Math.Abs(Sample.X - Position.X) / Width + Math.Abs(Sample.Y - Position.Z) / Width;
        }
        
        public override BoundingBox ToBoundingBox()
        {
            return new BoundingBox(Position.Xz(), Width);
        }
    }
}