using System;
using Hedra.Core;
using Hedra.Engine.Generation;
using OpenTK;

namespace Hedra.Engine.WorldBuilding
{
    public abstract class BasePlateau
    {
        protected BasePlateau(Vector2 Position)
        {
            this.Position = Position;
            this.MaxHeight = World.BiomePool.GetRegion(Position.ToVector3()).Generation.GetHeight(Position.X, Position.Y, null, out _);
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
            var addonHeight = this.MaxHeight * Math.Max(Final, 0f);

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