using Hedra.BiomeSystem;
using Hedra.Engine.PlantSystem;
using Hedra.Engine.PlantSystem.Harvestables;

namespace Hedra.Engine.BiomeSystem.NormalBiome
{
    public class NormalBiomeEnviromentDesign : BiomeEnviromentDesign
    {
        public NormalBiomeEnviromentDesign()
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
            };
        }

        public override PlacementDesign[] Designs { get; }
    }
}
