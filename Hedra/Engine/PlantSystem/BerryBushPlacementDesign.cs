using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using OpenTK;

namespace Hedra.Engine.PlantSystem
{
    public class BerryBushPlacementDesign : PlacementDesign
    {
        private readonly PlantDesign _berryBushDesign;

        public BerryBushPlacementDesign()
        {
            _berryBushDesign = new BerryBushDesign();
        }

        public override PlantDesign GetDesign(Vector3 Position, Chunk UnderChunk)
        {
            return _berryBushDesign;
        }

        public override bool ShouldPlace(Vector3 Position, Chunk UnderChunk)
        {
            return World.GetHighestBlockAt(Position.X, Position.Z).Type == BlockType.Grass &&
                   UnderChunk.Landscape.RandomGen.Next(0, 1750) == 1 && World.MenuSeed != World.Seed;
        }
    }
}
