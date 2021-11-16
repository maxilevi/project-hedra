using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class BerryBushCache : CacheType
    {
        public BerryBushCache()
        {
            AddModel(
                AssetManager.PLYLoader("Assets/Env/BerryBush.ply", Vector3.One) +
                AssetManager.PLYLoader("Assets/Env/Berries.ply", Vector3.One)
            );
        }

        public override CacheItem Type => CacheItem.BerryBush;
    }
}