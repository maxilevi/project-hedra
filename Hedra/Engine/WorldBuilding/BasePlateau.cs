using System;
using Hedra.Core;
using Hedra.Engine.Generation;
using System.Numerics;
using Hedra.Engine.BiomeSystem;
using Hedra.Numerics;

namespace Hedra.Engine.WorldBuilding
{
    public abstract class BasePlateau
    {
        protected const float BorderMultiplier = 1.5f;
        
        protected BasePlateau(Vector2 Position)
        {
            this.Position = Position;
            var region = World.BiomePool.GetRegion(Position.ToVector3());
            var height = region.Generation.GetAccurateMaxHeight(Position.X, Position.Y);
            this.MaxHeight = Math.Max(height, BiomePool.SeaLevel + 1);
        }

        public Vector2 Position { get; }
        
        public float MaxHeight { get; set; }
        
        public bool NoTrees { get; set; }
        
        public bool NoPlants { get; set; }

        public abstract bool Collides(Vector2 Point);

        public abstract float Density(Vector2 Point);
        
        public float Apply(Vector2 Point, float Height, out float Final, float SmallFrequency = 0)
        {
            Final = Density(Point);
            var addonHeight = (this.MaxHeight - Height) * Math.Max(Final, 0f);

            return Mathf.Lerp(
                Height,
                Math.Min(this.MaxHeight + SmallFrequency, Height + addonHeight),
                Final
            );
        }

        public abstract BoundingBox ToBoundingBox();
        
        public abstract BasePlateau Clone();
    }
}