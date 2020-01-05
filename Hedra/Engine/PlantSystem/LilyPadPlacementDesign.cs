using System;
using System.Numerics;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;

namespace Hedra.Engine.PlantSystem
{
    public class LilyPadPlacementDesign : MultipleStaticPlacementDesign
    {
        protected override PlantDesign[] Designs { get; } = new PlantDesign[]
        {
            new LilyPadDesign(),
        };
        
        protected override PlantDesign SelectDesign(Random Rng)
        {
            return Designs[0];
        }

        protected override BlockType[] Types { get; } = new[]
        {
            BlockType.Water
        };
        
        protected override bool ShouldPlace(Random Rng)
        {
            return Rng.Next(0, 500) == 1;
        }
    }
}