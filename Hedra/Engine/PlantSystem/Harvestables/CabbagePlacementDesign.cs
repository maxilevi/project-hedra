using System;
using Hedra.Engine.Generation;

namespace Hedra.Engine.PlantSystem.Harvestables
{
    public class CabbagePlacementDesign : CollectiblePlacementDesign
    {
        protected override PlantDesign Design { get; } = new CabbageDesign();

        protected override BlockType[] Types { get; } =
        {
            BlockType.Grass
        };

        protected override bool ShouldPlace(Random Rng)
        {
            return Rng.Next(0, 1500) == 1;
        }
    }
}