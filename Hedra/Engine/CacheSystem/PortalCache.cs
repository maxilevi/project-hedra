using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public class PortalCache : CacheType
    {
        public override CacheItem Type { get; } = CacheItem.Portal;

        public PortalCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Structures/Portal0.ply", Vector3.One));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Structures/Portal0.ply", Vector3.One));
        }
    }
}