using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class WillowTreesCache : CacheType
    {
        public WillowTreesCache()
        {
            var scale = Vector3.One * .5f;
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/Willow0.ply", scale));
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/Willow1.ply", scale));
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/Willow2.ply", scale));

            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/Willow0.ply", scale));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/Willow1.ply", scale));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/Willow2.ply", scale));
        }

        public override CacheItem Type => CacheItem.WillowTrees;
    }
}