using Hedra.Engine.PlantSystem;

namespace Hedra.Engine.BiomeSystem.UndeadBiome
{
    internal class UndeadBiomeEnviromentDesign : BiomeEnviromentDesign
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
