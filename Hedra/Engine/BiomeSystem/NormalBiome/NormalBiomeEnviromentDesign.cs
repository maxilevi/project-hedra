using Hedra.BiomeSystem;
using Hedra.Engine.PlantSystem;

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
            };
        }

        public override PlacementDesign[] Designs { get; }
    }
}
