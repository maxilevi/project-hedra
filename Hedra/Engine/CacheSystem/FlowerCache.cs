using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    [CacheIgnore]
    public class FlowerCache : CacheType
    {
        public FlowerCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Tetrahedra.ply", Vector3.One));
        }

        public override CacheItem Type => CacheItem.MaxEnums;
    }
}