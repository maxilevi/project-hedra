using Hedra.BiomeSystem;
using Hedra.Engine.PlantSystem;

namespace Hedra.Engine.BiomeSystem.ShroomDimension
{
    public class ShroomDimensionEnvironmentDesign : BiomeEnviromentDesign
    {
        public ShroomDimensionEnvironmentDesign()
        {
            Designs = new PlacementDesign[]
            {
                //new ReedPlacementDesign(),
                new RockPlacementDesign(),
               // new ShrubsPlacementDesign(),
                new MushroomPlacementDesign(),
                new TimberAndSticksPlacementDesign(),
                new PebblePlacementDesign(),
                new MushroomGrassPlacementDesign(),
                new WoodLogAndTreeStumpPlacementDesign()
            };
        }

        public override PlacementDesign[] Designs { get; }
    }
}