using System;
using System.Numerics;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;

namespace Hedra.Engine.PlantSystem
{
    public class ShrubsPlacementDesign : PlacementDesign
    {
        private readonly PlantDesign _bushDesign;
        private readonly PlantDesign _fernDesign;

        public ShrubsPlacementDesign()
        {
            _fernDesign = new FernDesign();
            _bushDesign = new BushDesign();
        }

        public override PlantDesign GetDesign(Vector3 Position, Chunk UnderChunk, Random Rng)
        {
            return Rng.Next(0, 4) != 1 ? _bushDesign : _fernDesign;
        }

        public override bool ShouldPlace(Vector3 Position, Chunk UnderChunk)
        {
            return World.GetHighestBlockAt(Position.X, Position.Z).Type == BlockType.Grass &&
                   UnderChunk.Landscape.RandomGen.Next(0, 450) == 1;
        }
    }
}