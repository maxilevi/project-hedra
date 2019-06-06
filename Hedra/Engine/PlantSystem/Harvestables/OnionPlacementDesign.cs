using System;
using Hedra.Engine.Generation;

namespace Hedra.Engine.PlantSystem.Harvestables
{
    public class OnionPlacementDesign : CollectiblePlacementDesign
    {
        protected override PlantDesign Design { get; } = new OnionDesign();

        protected override BlockType[] Types { get; } =
        {
            BlockType.Grass,
            BlockType.Dirt
        };

        protected override bool ShouldPlace(Random Rng) => Rng.Next(0, 1500) == 1;
    }
}