using Hedra.Engine.Management;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.CacheSystem
{
    public class PortalIconCache : CacheType
    {
        public override CacheItem Type { get; } = CacheItem.PortalIcon;

        public PortalIconCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Objects/Portal-Icon.ply", Vector3.One));
        }
    }
}