using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public class AppleTreesCache : CacheType
    {
        public AppleTreesCache()
        {
            this.AddModel(AssetManager.PLYLoader("Assets/Env/Trees/AppleTree0.ply", Vector3.One));
            this.AddModel(AssetManager.PLYLoader("Assets/Env/Trees/AppleTree1.ply", Vector3.One));
            this.AddModel(AssetManager.PLYLoader("Assets/Env/Trees/AppleTree2.ply", Vector3.One));

            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/AppleTree0.ply", 2, Vector3.One));
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/AppleTree1.ply", 2, Vector3.One));
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/AppleTree2.ply", 2, Vector3.One));
        }

        public override CacheItem Type => CacheItem.AppleTrees;
    }
}