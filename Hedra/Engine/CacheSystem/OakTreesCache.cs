using Hedra.Engine.Management;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.CacheSystem
{
    public class OakTreesCache : CacheType
    {
        public OakTreesCache()
        {
            this.AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/Oak0.ply", Vector3.One));
            this.AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/Oak1.ply", Vector3.One));
            this.AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/Oak2.ply", Vector3.One));

            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/Oak0.ply", 10, Vector3.One));
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/Oak1.ply", 2, Vector3.One));
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/Oak2.ply", 6, Vector3.One));
        }
        
        public override CacheItem Type => CacheItem.OakTrees;
    }
}
