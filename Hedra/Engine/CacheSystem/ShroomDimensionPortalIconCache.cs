using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class ShroomDimensionPortalIconCache : CacheType
    {
        public ShroomDimensionPortalIconCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Structures/TreeCircle/TreeCircle0-Icon.ply", Vector3.One));
        }

        public override CacheItem Type => CacheItem.ShroomPortalIcon;
    }
}