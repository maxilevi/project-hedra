using OpenToolkit.Mathematics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class GiantTreeCache : CacheType
    {
        public GiantTreeCache()
        {
            this.AddModel(AssetManager.PLYLoader("Assets/Env/Structures/GiantTree0.ply", Vector3.One));
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/GiantTree0.ply", Vector3.One));
        }
        
        public override CacheItem Type => CacheItem.GiantTree;
    }
}