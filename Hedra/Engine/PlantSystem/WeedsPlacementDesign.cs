
using System;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using System.Numerics;

namespace Hedra.Engine.PlantSystem
{
    public class WeedsPlacementDesign : PlacementDesign
    {
        private readonly PlantDesign _grassDesign;
        private readonly PlantDesign _wheatDesign;

        public WeedsPlacementDesign()
        {
            _grassDesign = new GrassDesign();
            _wheatDesign = new WheatDesign();
        }

        public override bool CanBeHidden => true;

        public override PlantDesign GetDesign(Vector3 Position, Chunk UnderChunk, Random Rng)
        {
            return Rng.Next(0, 7) != 1 ? _grassDesign : _wheatDesign;
        }

        public override bool ShouldPlace(Vector3 Position, Chunk UnderChunk)
        {
            return World.GetHighestBlockAt(Position.X, Position.Z).Type == BlockType.Grass &&
                World.GetNoise(Position.X * 0.045f, Position.Z * 0.045f) > 0.35f;
        }
    }
}
