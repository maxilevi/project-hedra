using System;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using System.Numerics;

namespace Hedra.Engine.PlantSystem
{
    public class DeadGrassPlacementDesign : PlacementDesign
    {
        private readonly DeadGrassDesign _design = new DeadGrassDesign();
        
        public override PlantDesign GetDesign(Vector3 Position, Chunk UnderChunk, Random Rng)
        {
            return _design;
        }

        public override bool ShouldPlace(Vector3 Position, Chunk UnderChunk)
        {
            return World.GetHighestBlockAt(Position.X, Position.Z).Type == BlockType.Grass &&
                World.GetNoise(Position.X * 0.045f, Position.Z * 0.045f) > 0.35f;
        }
    }
}