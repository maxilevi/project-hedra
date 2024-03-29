using System;
using System.Numerics;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Numerics;

namespace Hedra.Engine.PlantSystem
{
    public class TimberAndSticksPlacementDesign : MultipleCollectiblePlacementDesign
    {
        protected override PlantDesign[] Designs { get; } =
        {
            new TimberDesign(),
            new StickDesign()
        };

        protected override BlockType[] Types { get; } =
        {
            BlockType.Grass
        };

        private PlantDesign TimberDesign => Designs[0];

        private PlantDesign StickDesign => Designs[1];

        protected override PlantDesign SelectDesign(Random Rng)
        {
            return Rng.NextFloat() < .75f ? TimberDesign : StickDesign;
        }

        protected override bool ShouldPlace(Random Rng)
        {
            return Rng.Next(0, 2250) == 1;
        }

        public override bool ShouldPlace(Vector3 Position, Chunk UnderChunk)
        {
            return base.ShouldPlace(Position, UnderChunk)
                   && World.TreeGenerator.PlacementNoise(Position) > 0;
        }
    }
}