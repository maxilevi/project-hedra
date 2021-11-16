using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class MatCache : CacheType
    {
        public MatCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Mat0.ply", Vector3.One));

            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Mat0.ply", 2, Vector3.One));
        }

        public override CacheItem Type => CacheItem.Mat;
    }
}