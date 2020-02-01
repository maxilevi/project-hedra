using Hedra.BiomeSystem;
using Hedra.Engine.PlantSystem;

namespace Hedra.Engine.BiomeSystem.UndeadBiome
{
    public class GhostTownEnvironmentDesign : BiomeEnviromentDesign
    {
        public GhostTownEnvironmentDesign()
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
                new WoodLogAndTreeStumpPlacementDesign(),
            };
        }

        public override PlacementDesign[] Designs { get; }
    }
}
