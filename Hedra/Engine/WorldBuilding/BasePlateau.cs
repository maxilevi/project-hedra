using System;
using Hedra.Core;
using Hedra.Engine.Generation;
using OpenTK;

namespace Hedra.Engine.WorldBuilding
{
    public abstract class BasePlateau
    {
        protected BasePlateau(Vector3 Position)
        {
            this.Position = Position;
            this.MaxHeight = World.BiomePool.GetRegion(Position).Generation.GetHeight(Position.X, Position.Z, null, out _);
        }

        public Vector3 Position { get; }
        
        public float MaxHeight { get; set; }
        
        public bool NoTrees { get; set; }
        
        public bool NoPlants { get; set; }
        
        public abstract bool Collides(Vector2 Point);

        protected abstract float Density(Vector2 Point);
        
        public float Apply(Vector2 Point, float Height, out float Final, float SmallFrequency = 0)
        {
            Final = Density(Point); 
            var addonHeight = this.MaxHeight * Math.Max(Final, 0f);

            Height += addonHeight;
            return Mathf.Lerp(
                Height - addonHeight,
                Math.Min(this.MaxHeight + SmallFrequency, Height),
                Final
            );
        }

        public abstract BasePlateau Clone();
    }
}