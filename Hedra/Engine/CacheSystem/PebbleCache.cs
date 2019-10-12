using Hedra.Engine.Management;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.CacheSystem
{
    public class PebbleCache : CacheType
    {
        public override CacheItem Type => CacheItem.Pebble;

        public PebbleCache()
        {
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Plants/Pebble0.ply", Vector3.One));
        }
    }
}