using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class OnionPlantCache : CacheType
    {
        public OnionPlantCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Plants/Onion0.ply", Vector3.One * 1.25f));
        }

        public override CacheItem Type => CacheItem.Onion;
    }
}