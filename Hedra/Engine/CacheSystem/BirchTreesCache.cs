using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class BirchTreesCache : CacheType
    {
        public BirchTreesCache()
        {
            var scale = Vector3.One * .75f;
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/Birch0.ply", scale));
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/Birch1.ply", scale));
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/Birch2.ply", scale));
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/Birch3.ply", scale));

            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/Birch0.ply", scale));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/Birch1.ply", scale));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/Birch2.ply", scale));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/Birch3.ply", scale));
        }

        public override CacheItem Type => CacheItem.BirchTrees;
    }
}