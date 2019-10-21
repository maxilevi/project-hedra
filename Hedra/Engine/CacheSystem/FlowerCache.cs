using Hedra.Engine.Management;
using System.Numerics;

namespace Hedra.Engine.CacheSystem
{
    [CacheIgnore]
    public class FlowerCache : CacheType
    {
        public FlowerCache()
        {
            this.AddModel(AssetManager.PLYLoader("Assets/Env/Tetrahedra.ply", Vector3.One));
        }
        
        public override CacheItem Type => CacheItem.MaxEnums;
    }
}
