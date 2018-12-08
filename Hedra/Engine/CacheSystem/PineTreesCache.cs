using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    class PineTreesCache : CacheType
    {
        public PineTreesCache()
        {
            this.AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/Pine0.ply", Vector3.One));
            this.AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/Pine1.ply", Vector3.One));
            this.AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/Pine2.ply", Vector3.One));
            this.AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/Pine3.ply", Vector3.One));

            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/Pine0.ply", 2, Vector3.One));
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/Pine1.ply", 3, Vector3.One));
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/Pine2.ply", 2, Vector3.One));
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/Pine3.ply", 2, Vector3.One));
        }
        
        public override CacheItem Type => CacheItem.PineTrees;
    }
}