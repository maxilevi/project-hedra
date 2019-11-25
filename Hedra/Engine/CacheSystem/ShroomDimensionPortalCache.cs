using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class ShroomDimensionPortalCache : CacheType
    {
        public override CacheItem Type => CacheItem.ShroomPortal;

        public ShroomDimensionPortalCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Structures/TreeCircle/TreeCircle0.ply", Vector3.One));
            
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Structures/TreeCircle/TreeCircle0.ply", Vector3.One));
        }
    }
}