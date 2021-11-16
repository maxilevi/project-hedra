using Hedra.Engine.ModuleSystem.Templates;

namespace Hedra.BiomeSystem
{
    public class RegionMob
    {
        private readonly BiomeMobDesign _design;
        private readonly int _seed;

        public RegionMob(int Seed, BiomeMobDesign Design)
        {
            _seed = Seed;
            _design = Design;
        }

        public SpawnerSettings SpawnerSettings => _design.Settings;

        public static RegionMob Interpolate(params RegionMob[] Regions)
        {
            return Regions[0];
        }
    }
}