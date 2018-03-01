using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public class DeadTreesCache : CacheType
    {
        public DeadTreesCache()
        {
            this.AddModel(AssetManager.PlyLoader("Assets/Env/DeadTree0.ply", Vector3.One));

            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/DeadTree0.ply", 2, Vector3.One));
        }
    }
}