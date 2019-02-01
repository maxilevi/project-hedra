using System;
using Hedra.Engine.Generation;

namespace Hedra.Engine.PlantSystem
{
    public class MushroomPlacementDesign : CollectiblePlacementDesign
    {
        protected override PlantDesign Design { get; } = new MushroomDesign();
        
        protected override BlockType[] Types { get; } =
        {
            BlockType.Dirt,
        };

        protected override bool ShouldPlace(Random Rng) => Rng.Next(0, 1000) == 1;
    }
}
