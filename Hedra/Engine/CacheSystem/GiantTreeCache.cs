using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class GiantTreeCache : CacheType
    {
        public GiantTreeCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Structures/GiantTree0.ply", Vector3.One));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/GiantTree0.ply", Vector3.One));
        }

        public override CacheItem Type => CacheItem.GiantTree;
    }
}