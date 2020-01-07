using System.Numerics;
using Hedra.Engine.Management;
using Hedra.Engine.TreeSystem;
using Hedra.Rendering;

namespace Hedra.Engine.CacheSystem
{
    public class BirchTreesCache : CacheType
    {
        public override CacheItem Type => CacheItem.BirchTrees;

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
    }
}