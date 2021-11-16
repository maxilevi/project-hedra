using System;
using Hedra.Engine.Generation;

namespace Hedra.Engine.PlantSystem
{
    public class WoodLogAndTreeStumpPlacementDesign : MultipleStaticPlacementDesign
    {
        protected override BlockType[] Types { get; } =
        {
            BlockType.Dirt,
            BlockType.Grass
        };

        protected override PlantDesign[] Designs { get; } =
        {
            new TreeStumpDesign(),
            new WoodLogDesign()
        };

        protected override bool ShouldPlace(Random Rng)
        {
            return Rng.Next(0, 30000) == 1;
        }

        protected override PlantDesign SelectDesign(Random Rng)
        {
            return Rng.Next(0, 4) == 1 ? Designs[0] : Designs[1];
        }
    }
}