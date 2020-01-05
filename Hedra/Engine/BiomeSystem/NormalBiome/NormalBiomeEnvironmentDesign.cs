using Hedra.BiomeSystem;
using Hedra.Engine.PlantSystem;
using Hedra.Engine.PlantSystem.Harvestables;

namespace Hedra.Engine.BiomeSystem.NormalBiome
{
    public class NormalBiomeEnvironmentDesign : BiomeEnviromentDesign
    {
        public NormalBiomeEnvironmentDesign()
        {
            Designs = new PlacementDesign[]
            {
                new ReedPlacementDesign(),
                new WeedsPlacementDesign(),
                new RockPlacementDesign(),
                new CloudPlacementDesign(),
                new ShrubsPlacementDesign(),
                new BerryBushPlacementDesign(),
                new MushroomPlacementDesign(),
                new TimberAndSticksPlacementDesign(),
                new PebblePlacementDesign(),
                /* Harvestables */
                new CarrotPlacementDesign(), 
                new OnionPlacementDesign(), 
                new WoodLogAndTreeStumpPlacementDesign(),
                new LilyPadPlacementDesign(),
                new RosemaryAndThymePlacementDesign()
            };
        }

        public override PlacementDesign[] Designs { get; }
    }
}
