using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class PortalIconCache : CacheType
    {
        public PortalIconCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Objects/Portal-Icon.ply", Vector3.One));
        }

        public override CacheItem Type { get; } = CacheItem.PortalIcon;
    }
}