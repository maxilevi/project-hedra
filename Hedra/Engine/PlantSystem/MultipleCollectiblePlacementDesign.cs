using System;
using System.Numerics;
using Hedra.Engine.Generation.ChunkSystem;

namespace Hedra.Engine.PlantSystem
{
    public abstract class MultipleCollectiblePlacementDesign : CollectiblePlacementDesign
    {
        protected override PlantDesign Design => throw new NotImplementedException();

        protected abstract PlantDesign[] Designs { get; }

        protected abstract PlantDesign SelectDesign(Random Rng);

        public override PlantDesign GetDesign(Vector3 Position, Chunk UnderChunk, Random Rng)
        {
            return SelectDesign(Rng);
        }
    }
}