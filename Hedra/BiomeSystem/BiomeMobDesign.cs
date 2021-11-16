using Hedra.Engine.Management;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.ModuleSystem.Templates;

namespace Hedra.BiomeSystem
{
    public abstract class BiomeMobDesign
    {
        protected BiomeMobDesign()
        {
            WorldModulesReload(AssetManager.AppPath);
            World.ModulesReload += WorldModulesReload;
        }

        public SpawnerSettings Settings { get; private set; }

        protected abstract string Name { get; }

        private void WorldModulesReload(string AppPath)
        {
            Settings = SpawnerLoader.Load(AppPath, Name);
        }
    }
}