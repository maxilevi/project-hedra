using System;
using Hedra.Engine.Generation;

namespace Hedra.Engine.PlantSystem.Harvestables
{
    public class RosemaryAndThymePlacementDesign : MultipleCollectiblePlacementDesign
    {
        protected override BlockType[] Types { get; } = new[]
        {
            BlockType.Dirt,
            BlockType.Grass,
            BlockType.Stone
        };
        
        protected override bool ShouldPlace(Random Rng)
        {
            return Rng.Next(0, 40000) == 1;
        }

        protected override PlantDesign[] Designs { get; } = new PlantDesign[]
        {
            new RosemaryDesign(),
            new ThymeDesign(),
        };
        
        private PlantDesign Rosemary => Designs[0];
        private PlantDesign Thyme => Designs[1];
        
        protected override PlantDesign SelectDesign(Random Rng)
        {
            return Rng.Next(0, 3) == 1 ? Thyme : Rosemary;
        }
    }
}