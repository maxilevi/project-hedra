using Hedra.BiomeSystem;
using Hedra.Engine.PlantSystem;

namespace Hedra.Engine.BiomeSystem.DesertBiome
{
    public class DesertBiomeEnviromentDesign : BiomeEnviromentDesign
    {
        public DesertBiomeEnviromentDesign()
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
