using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class TimberCache : CacheType
    {
        public TimberCache()
        {
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Plants/Timber0.ply", Vector3.One));
        }

        public override CacheItem Type => CacheItem.Timber;
    }
}