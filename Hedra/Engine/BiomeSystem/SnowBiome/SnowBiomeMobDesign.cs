using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.ModuleSystem;

namespace Hedra.Engine.BiomeSystem.SnowBiome
{
    public class SnowBiomeMobDesign : BiomeMobDesign
    {
        private SpawnerSettings _settings;
        public override SpawnerSettings Settings => _settings;

        public SnowBiomeMobDesign()
        {
            this.WorldModulesReload(AssetManager.AppPath);
            World.ModulesReload += this.WorldModulesReload;
        }

        private void WorldModulesReload(string AppPath)
        {
            _settings = SpawnerLoader.Load(AppPath, "SnowBiome");
        }
    }
}
