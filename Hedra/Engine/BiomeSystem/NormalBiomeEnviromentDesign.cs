using Hedra.Engine.Generation;
using Hedra.Engine.PlantSystem;
using Hedra.Engine.TreeSystem;

namespace Hedra.Engine.BiomeSystem
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
                new BerryBushPlacementDesign(),
            };
        }

        public override PlacementDesign[] Designs { get; }
    }
}
