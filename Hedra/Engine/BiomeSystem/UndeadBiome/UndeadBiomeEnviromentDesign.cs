using Hedra.Engine.PlantSystem;

namespace Hedra.Engine.BiomeSystem.UndeadBiome
{
    public class UndeadBiomeEnviromentDesign : BiomeEnviromentDesign
    {
        public UndeadBiomeEnviromentDesign()
        {
            Designs = new PlacementDesign[]
            {
                new RockPlacementDesign(),
            };
        }

        public override PlacementDesign[] Designs { get; }
    }
}
