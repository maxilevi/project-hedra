using System;
using Hedra.Engine.Generation;

namespace Hedra.Engine.PlantSystem
{
    public class BerryBushPlacementDesign : CollectiblePlacementDesign
    {
        protected override PlantDesign Design { get; } = new BerryBushDesign();

        protected override BlockType[] Types { get; } =
        {
            BlockType.Grass
        };

        protected override bool ShouldPlace(Random Rng)
        {
            return Rng.Next(0, 3000) == 1;
        }
    }
}