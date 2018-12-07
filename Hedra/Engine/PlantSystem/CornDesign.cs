using System;
using Hedra.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Rendering;

namespace Hedra.Engine.PlantSystem
{
    public class CornDesign : FarmPlantDesign
    {
        protected override CacheItem Type => CacheItem.Corn;
        public override VertexData Paint(VertexData Data, Region Region, Random Rng)
        {
            throw new NotImplementedException();
        }
    }
}