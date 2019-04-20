using Hedra.BiomeSystem;
using Hedra.Engine.BiomeSystem.UndeadBiome;

namespace Hedra.Engine.BiomeSystem.GhostTown
{
    public class GhostTownSkyDesign : UndeadBiomeSkyDesign
    {
        public override float MinLight(int Seed)
        {
            return 0.4f;
        }

        public override float MaxLight(int Seed)
        {
            return 0.4f;
        }
    }
}