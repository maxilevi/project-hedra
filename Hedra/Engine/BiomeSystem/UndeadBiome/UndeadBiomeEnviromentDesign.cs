using Hedra.BiomeSystem;
using Hedra.Engine.PlantSystem;

namespace Hedra.Engine.BiomeSystem.UndeadBiome
{
    public class UndeadBiomeEnviromentDesign : BiomeEnviromentDesign
    {
        public UndeadBiomeEnviromentDesign()
        {
            Designs = new PlacementDesign[]
            {
                new ReedPlacementDesign(),
                new RockPlacementDesign(),
                new ShrubsPlacementDesign(),
                new MushroomPlacementDesign(),
                new TimberAndSticksPlacementDesign(),
                new PebblePlacementDesign(),
                new DeadGrassPlacementDesign(), 
            };
        }

        public override PlacementDesign[] Designs { get; }
    }
}
