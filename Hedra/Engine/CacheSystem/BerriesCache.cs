using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    internal class BerriesCache : CacheType
    {
        public BerriesCache()
        {
            this.AddModel(AssetManager.PlyLoader("Assets/Env/Berries.ply", Vector3.One));
        }
    }
}