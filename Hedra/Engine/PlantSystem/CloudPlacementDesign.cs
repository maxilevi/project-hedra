using System;
using System.Numerics;
using Hedra.Engine.Generation.ChunkSystem;

namespace Hedra.Engine.PlantSystem
{
    public class CloudPlacementDesign : PlacementDesign
    {
        private readonly PlantDesign _cloudDesign;

        public CloudPlacementDesign()
        {
            _cloudDesign = new CloudDesign();
        }

        public override PlantDesign GetDesign(Vector3 Position, Chunk UnderChunk, Random Rng)
        {
            return _cloudDesign;
        }

        public override bool ShouldPlace(Vector3 Position, Chunk UnderChunk)
        {
            return UnderChunk.Landscape.RandomGen.Next(0, 15000) == 1;
        }
    }
}