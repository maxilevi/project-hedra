using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    internal class BerryBushCache : CacheType
    {
        public BerryBushCache()
        {
            this.AddModel(AssetManager.PlyLoader("Assets/Env/BerryBush.ply", Vector3.One));
        }
    }
}
