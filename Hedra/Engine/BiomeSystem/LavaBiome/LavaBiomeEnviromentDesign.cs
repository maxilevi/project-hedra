using Hedra.Engine.PlantSystem;

namespace Hedra.Engine.BiomeSystem.LavaBiome
{
    public class LavaBiomeEnviromentDesign : BiomeEnviromentDesign
    {
        public LavaBiomeEnviromentDesign()
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
