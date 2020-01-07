using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class WillowTreesCache : CacheType
    {
        public override CacheItem Type => CacheItem.WillowTrees;
        
        public WillowTreesCache()
        {
            var scale = Vector3.One * .5f;
            this.AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/Willow0.ply", scale));
            this.AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/Willow1.ply", scale));
            this.AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/Willow2.ply", scale));

            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/Willow0.ply", scale));
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/Willow1.ply", scale));
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/Willow2.ply", scale));
        }
    }
}