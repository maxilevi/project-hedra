using Hedra.Engine.PlantSystem;

namespace Hedra.Engine.BiomeSystem.NormalBiome
{
    public class NormalBiomeEnviromentDesign : BiomeEnviromentDesign
    {
        public NormalBiomeEnviromentDesign()
        {
            Designs = new PlacementDesign[]
            {
                new WeedsPlacementDesign(),
                new RockPlacementDesign(), 
                new CloudPlacementDesign(), 
                new ShrubsPlacementDesign(),
                new BerryBushPlacementDesign()
            };
        }

        public override PlacementDesign[] Designs { get; }
    }
}
