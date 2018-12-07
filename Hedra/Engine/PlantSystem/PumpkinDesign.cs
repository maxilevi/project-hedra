using System;
using Hedra.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Rendering;

namespace Hedra.Engine.PlantSystem
{
    public class PumpkinDesign : FarmPlantDesign
    {
        protected override CacheItem Type => CacheItem.Pumpkin;
        public override VertexData Paint(VertexData Data, Region Region, Random Rng)
        {
            throw new NotImplementedException();
        }
    }
}