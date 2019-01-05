using System;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using OpenTK;

namespace Hedra.Engine.PlantSystem
{
    public class TimberPlacementDesign : CollectibleDesign
    {
        protected override PlantDesign Design { get; } = new TimberDesign();
        
        protected override BlockType Type => BlockType.Grass;

        protected override bool ShouldPlace(Random Rng) => Rng.Next(0, 3000) == 1;

        public override bool ShouldPlace(Vector3 Position, Chunk UnderChunk)
        {
            return base.ShouldPlace(Position, UnderChunk)
                   && World.TreeGenerator.SpaceNoise(Position.X, Position.Z) > 0;
        }
    }
}