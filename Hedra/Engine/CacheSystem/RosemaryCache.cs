using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class RosemaryCache : CacheType
    {
        public RosemaryCache()
        {
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Plants/Rosemary0.ply", Vector3.One * 1.5f));
        }

        public override CacheItem Type => CacheItem.Rosemary;
    }
}