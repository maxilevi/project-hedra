using Hedra.Engine.Management;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.CacheSystem
{
    public class WellCache : CacheType
    {
        public override CacheItem Type => CacheItem.Well;

        public WellCache()
        {
            base.AddModel(AssetManager.PLYLoader("Assets/Env/Village/Well0.ply", Vector3.One * 3));
            
            base.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Village/Well0.ply", Vector3.One * 3));
        }
    }
}