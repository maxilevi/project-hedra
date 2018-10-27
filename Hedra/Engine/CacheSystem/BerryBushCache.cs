using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public class BerryBushCache : CacheType
    {
        public BerryBushCache()
        {
            this.AddModel(AssetManager.PLYLoader("Assets/Env/BerryBush.ply", Vector3.One));
        }
    }
}
