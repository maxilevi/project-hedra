using Hedra.Engine.ModuleSystem;
using Hedra.Engine.ModuleSystem.Templates;

namespace Hedra.BiomeSystem
{
    public class RegionMob
    {
        private readonly int _seed;
        private readonly BiomeMobDesign _design;

        public RegionMob(int Seed, BiomeMobDesign Design)
        {
            this._seed = Seed;
            this._design = Design;
        }

        public SpawnerSettings SpawnerSettings => _design.Settings;

        public static RegionMob Interpolate(params RegionMob[] Regions)
        {
            return Regions[0];
        }
    }
}
