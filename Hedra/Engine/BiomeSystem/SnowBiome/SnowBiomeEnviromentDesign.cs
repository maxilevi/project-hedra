using Hedra.Engine.PlantSystem;

namespace Hedra.Engine.BiomeSystem.SnowBiome
{
    public class SnowBiomeEnviromentDesign : BiomeEnviromentDesign
    {
        public SnowBiomeEnviromentDesign()
        {
            Designs = new PlacementDesign[]
            {
                new WeedsPlacementDesign(),
                new RockPlacementDesign(),
                new CloudPlacementDesign(),
                new ShrubsPlacementDesign(),
                new BerryBushPlacementDesign(),
            };
        }

        public override PlacementDesign[] Designs { get; }
    }
}
