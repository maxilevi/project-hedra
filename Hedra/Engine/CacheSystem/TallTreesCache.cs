using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public class TallTreesCache : CacheType
    {
        public TallTreesCache()
        {
            this.AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/TallTree0.ply", Vector3.One));
            this.AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/TallTree1.ply", Vector3.One));
            this.AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/TallTree2.ply", Vector3.One));

            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/TallTree0.ply", 2, Vector3.One));
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/TallTree1.ply", 2, Vector3.One));
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/TallTree2.ply", 2, Vector3.One));
        }
        
        public override CacheItem Type => CacheItem.TallTrees;
    }
}