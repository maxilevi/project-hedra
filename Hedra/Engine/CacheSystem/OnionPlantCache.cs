using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public class OnionPlantCache : CacheType
    {
        public override CacheItem Type => CacheItem.Onion;

        public OnionPlantCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Plants/Onion0.ply", Vector3.One));
        }
    }
}