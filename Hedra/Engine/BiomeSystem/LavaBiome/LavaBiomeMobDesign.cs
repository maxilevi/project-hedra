using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.ModuleSystem;

namespace Hedra.Engine.BiomeSystem.LavaBiome
{
    public class LavaBiomeMobDesign : BiomeMobDesign
    {
        private SpawnerSettings _settings;
        public override SpawnerSettings Settings => _settings;

        public LavaBiomeMobDesign()
        {
            this.WorldModulesReload(AssetManager.AppPath);
            World.ModulesReload += this.WorldModulesReload;
        }

        private void WorldModulesReload(string AppPath)
        {
            _settings = SpawnerLoader.Load(AppPath, "LavaBiome");
        }
    }
}
