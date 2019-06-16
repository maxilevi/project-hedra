using System;
using Hedra.Engine.Generation;

namespace Hedra.Engine.PlantSystem.Harvestables
{
    public class TomatoPlacementDesign : CollectiblePlacementDesign
    {
        protected override PlantDesign Design { get; } = new TomatoDesign();

        protected override BlockType[] Types { get; } =
        {
            BlockType.Grass,
            BlockType.Dirt
        };

        protected override bool ShouldPlace(Random Rng) => Rng.Next(0, 1500) == 1;
    }
}