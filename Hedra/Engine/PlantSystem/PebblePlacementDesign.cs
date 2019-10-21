using System;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using System.Numerics;

namespace Hedra.Engine.PlantSystem
{
    public class PebblePlacementDesign : CollectiblePlacementDesign
    {
        protected override PlantDesign Design { get; } = new PebbleDesign();

        protected override BlockType[] Types { get; } =
        {
            BlockType.Grass,
            BlockType.Stone,
        };

        protected override bool ShouldPlace(Random Rng) => Rng.Next(0, 3000) == 1;
    }
}