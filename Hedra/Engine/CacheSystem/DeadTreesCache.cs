using Hedra.Engine.Management;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.CacheSystem
{
    public class DeadTreesCache : CacheType
    {
        public DeadTreesCache()
        {
            this.AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/DeadTree0.ply", Vector3.One));
            this.AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/DeadTree1.ply", Vector3.One));
            this.AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/DeadTree2.ply", Vector3.One));
            this.AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/DeadTree3.ply", Vector3.One));

            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/DeadTree0.ply", 2, Vector3.One));
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/DeadTree0.ply", 2, Vector3.One));
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/DeadTree0.ply", 2, Vector3.One));
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/DeadTree0.ply", 2, Vector3.One));
        }
        
        public override CacheItem Type => CacheItem.DeadTrees;
    }
}