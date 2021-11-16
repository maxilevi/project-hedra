using System;
using System.Numerics;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;

namespace Hedra.Engine.PlantSystem
{
    public class LilyPadPlacementDesign : PlacementDesign
    {
        private readonly PlantDesign _lilyPadDesign = new LilyPadDesign();
        public override bool CanBePlacedOnWater => true;

        public override PlantDesign GetDesign(Vector3 Position, Chunk UnderChunk, Random Rng)
        {
            return _lilyPadDesign;
        }

        public override bool ShouldPlace(Vector3 Position, Chunk UnderChunk)
        {
            return Physics.IsWaterBlock(Position)
                   && UnderChunk.Landscape.RandomGen.Next(0, 20) == 1
                   && World.GetNoise(Position.X * 0.005f, Position.Z * 0.005f) > 0.3f;
        }
    }
}