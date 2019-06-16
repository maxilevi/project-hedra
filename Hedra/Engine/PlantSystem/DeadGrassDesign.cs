using System;
using Hedra.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.PlantSystem
{
    public class DeadGrassDesign : WeedDesign
    {
        public override CacheItem Type => CacheItem.DeadGrass;
        
        protected override void ApplyPaint(VertexData Data, Region Region, Random Rng)
        {
            
        }
    }
}