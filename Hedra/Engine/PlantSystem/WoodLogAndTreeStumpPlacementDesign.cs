using System;
using System.Numerics;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;

namespace Hedra.Engine.PlantSystem
{
    public class WoodLogAndTreeStumpPlacementDesign : MultipleStaticPlacementDesign
    {
        protected override BlockType[] Types { get; } = new[]
        {
            BlockType.Dirt,
            BlockType.Grass
        };
        
        protected override bool ShouldPlace(Random Rng)
        {
            return Rng.Next(0, 30000) == 1;
        }

        protected override PlantDesign[] Designs { get; } = new PlantDesign[]
        {
            new TreeStumpDesign(),
            new WoodLogDesign()
        };
        
        protected override PlantDesign SelectDesign(Random Rng)
        {
            return Rng.Next(0, 4) == 1 ? Designs[0] : Designs[1];
        }
    }
}