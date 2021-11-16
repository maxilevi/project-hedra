using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class WellCache : CacheType
    {
        public WellCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Village/Well0.ply", Vector3.One * 3));

            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Village/Well0.ply", Vector3.One * 3));
        }

        public override CacheItem Type => CacheItem.Well;
    }
}