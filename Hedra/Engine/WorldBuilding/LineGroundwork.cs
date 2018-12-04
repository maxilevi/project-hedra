using System;
using Hedra.Engine.Generation;
using OpenTK;

namespace Hedra.Engine.WorldBuilding
{
    public class LineGroundwork : BaseGroundwork
    {
        public float Width { get; set; } = 14f;
        private Vector2 Origin { get; set; }
        private Vector2 End { get; set; }
        public override float BonusHeight { get; set; } = -1.0f;
        public override bool IsPath => true;

        public LineGroundwork(Vector2 Origin, Vector2 End, BlockType Type = BlockType.Path)
        {
            this.Origin = Origin;
            this.End = End;
            this.Type = Type;
        }

        public override bool Affects(Vector2 Sample)
        {
            var length = (End - Origin).LengthFast;
            var dir = (End - Origin) * (1f / length);
            var point = (Sample - Origin).LengthFast;           
            return point < length && (point * dir + Origin - Sample).LengthFast < Width;
        }
        
        public override float Density(Vector2 Sample)
        {
            var dir = (End - Origin).NormalizedFast();
            var point = (Sample - Origin).LengthFast;
            var den = 1 - Math.Min((point * dir + Origin - Sample).LengthFast / Width, 1);
            return Math.Min(den * 2.5f, 1f);
        }
    }
}
