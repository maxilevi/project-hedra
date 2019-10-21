using Hedra.Engine.Management;
using System.Numerics;

namespace Hedra.Engine.CacheSystem
{
    public class TimberCache : CacheType
    {
        public override CacheItem Type => CacheItem.Timber;

        public TimberCache()
        {
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Plants/Timber0.ply", Vector3.One));
        }
    }
}