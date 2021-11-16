using System;
using Hedra.Engine.Generation;

namespace Hedra.Engine.PlantSystem.Harvestables
{
    public class PeasPlacementDesign : CollectiblePlacementDesign
    {
        protected override PlantDesign Design { get; } = new PeasDesign();

        protected override BlockType[] Types { get; } =
        {
            BlockType.Grass,
            BlockType.Dirt
        };

        protected override bool ShouldPlace(Random Rng)
        {
            return Rng.Next(0, 1500) == 1;
        }
    }
}