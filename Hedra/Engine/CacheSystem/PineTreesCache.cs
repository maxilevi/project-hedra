using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    class PineTreesCache : CacheType
    {
        public PineTreesCache()
        {
            this.AddModel(AssetManager.PlyLoader("Assets/Env/Pine0.ply", Vector3.One));
            this.AddModel(AssetManager.PlyLoader("Assets/Env/Pine1.ply", Vector3.One));
            this.AddModel(AssetManager.PlyLoader("Assets/Env/Pine2.ply", Vector3.One));
            this.AddModel(AssetManager.PlyLoader("Assets/Env/Pine3.ply", Vector3.One));

            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Pine0.ply", 2, Vector3.One));
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Pine1.ply", 3, Vector3.One));
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Pine2.ply", 2, Vector3.One));
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Pine3.ply", 2, Vector3.One));
        }
    }
}