using Hedra.Engine.Management;
using OpenToolkit.Mathematics;

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