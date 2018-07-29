using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    internal class AppleTreesCache : CacheType
    {
        public AppleTreesCache()
        {
            this.AddModel(AssetManager.PLYLoader("Assets/Env/AppleTree0.ply", Vector3.One));
            this.AddModel(AssetManager.PLYLoader("Assets/Env/AppleTree1.ply", Vector3.One));
            this.AddModel(AssetManager.PLYLoader("Assets/Env/AppleTree2.ply", Vector3.One));

            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/AppleTree0.ply", 2, Vector3.One));
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/AppleTree1.ply", 2, Vector3.One));
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/AppleTree2.ply", 2, Vector3.One));
        }
    }
}