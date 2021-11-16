using System;
using Hedra.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.PlantSystem
{
    public class DeadGrassDesign : WeedDesign
    {
        public override CacheItem Type => CacheItem.DeadGrass;

        protected override void ApplyPaint(NativeVertexData Data, Region Region, Random Rng)
        {
        }
    }
}