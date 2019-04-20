using Hedra.Engine.Management;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.ModuleSystem.Templates;

namespace Hedra.BiomeSystem
{
    public abstract class BiomeMobDesign
    {
        public SpawnerSettings Settings { get; private set; }

        protected BiomeMobDesign()
        {
            this.WorldModulesReload(AssetManager.AppPath);
            World.ModulesReload += this.WorldModulesReload;
        }

        private void WorldModulesReload(string AppPath)
        {
            Settings = SpawnerLoader.Load(AppPath, Name);
        }
        
        protected abstract string Name { get; }
    }
}
