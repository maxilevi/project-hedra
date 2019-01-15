using Hedra.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.ModuleSystem.Templates;

namespace Hedra.Engine.BiomeSystem.DesertBiome
{
    public class DesertBiomeMobDesign : BiomeMobDesign
    {
        private SpawnerSettings _settings;
        public override SpawnerSettings Settings => _settings;

        public DesertBiomeMobDesign()
        {
            this.WorldModulesReload(AssetManager.AppPath);
            World.ModulesReload += this.WorldModulesReload;
        }

        private void WorldModulesReload(string AppPath)
        {
            _settings = SpawnerLoader.Load(AppPath, "DesertBiome");
        }
    }
}
