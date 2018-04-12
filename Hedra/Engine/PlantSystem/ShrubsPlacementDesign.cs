using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using OpenTK;

namespace Hedra.Engine.PlantSystem
{
    public class ShrubsPlacementDesign : PlacementDesign
    {
        private readonly PlantDesign _fernDesign;
        private readonly PlantDesign _bushDesign;

        public ShrubsPlacementDesign()
        {
            _fernDesign = new FernDesign();
            _bushDesign = new BushDesign();
        }

        public override PlantDesign GetDesign(Vector3 Position, Chunk UnderChunk)
        {
            return UnderChunk.Landscape.RandomGen.Next(0, 4) != 1 ? _bushDesign : _fernDesign;
        }

        public override bool ShouldPlace(Vector3 Position, Chunk UnderChunk)
        {
            return World.GetHighestBlockAt(Position.X, Position.Z).Type == BlockType.Grass &&
                   UnderChunk.Landscape.RandomGen.Next(0, 450) == 1;
        }
    }
}
