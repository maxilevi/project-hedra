using System;
using Hedra.Engine.Generation;

namespace Hedra.Engine.PlantSystem.Harvestables
{
    public class CarrotPlacementDesign : CollectiblePlacementDesign
    {
        protected override PlantDesign Design { get; } = new CarrotDesign();

        protected override BlockType[] Types { get; } =
        {
            BlockType.Grass,
            BlockType.Dirt
        };

        protected override bool ShouldPlace(Random Rng)
        {
            return Rng.Next(0, 2000) == 1;
        }
    }
}