using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class CampfireIconCache : CacheType
    {
        public CampfireIconCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/UI/CampfireIcon.ply", Vector3.One * .75f));
        }

        public override CacheItem Type => CacheItem.CampfireIcon;
    }
}