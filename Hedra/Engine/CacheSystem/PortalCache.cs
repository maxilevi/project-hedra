using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class PortalCache : CacheType
    {
        public PortalCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Structures/Portal0.ply", Vector3.One));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Structures/Portal0.ply", Vector3.One));
        }

        public override CacheItem Type { get; } = CacheItem.Portal;
    }
}