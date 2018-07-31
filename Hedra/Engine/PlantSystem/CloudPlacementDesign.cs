using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using OpenTK;

namespace Hedra.Engine.PlantSystem
{
    public class CloudPlacementDesign : PlacementDesign
    {
        private readonly PlantDesign _cloudDesign;

        public CloudPlacementDesign()
        {
            _cloudDesign = new CloudDesign();
        }

        public override PlantDesign GetDesign(Vector3 Position, Chunk UnderChunk)
        {
            return _cloudDesign;
        }

        public override bool ShouldPlace(Vector3 Position, Chunk UnderChunk)
        {
            return UnderChunk.Landscape.RandomGen.Next(0, 15000) == 1;
        }
    }
}
