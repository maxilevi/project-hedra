using Hedra.Engine.PlantSystem;

namespace Hedra.Engine.BiomeSystem.SnowBiome
{
    internal class SnowBiomeEnviromentDesign : BiomeEnviromentDesign
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
