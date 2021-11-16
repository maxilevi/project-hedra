using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class PebbleCache : CacheType
    {
        public PebbleCache()
        {
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Plants/Pebble0.ply", Vector3.One));
        }

        public override CacheItem Type => CacheItem.Pebble;
    }
}