using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class AppleTreesCache : CacheType
    {
        public AppleTreesCache()
        {
            var scale = Vector3.One * 1.5f;
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/AppleTree0.ply", scale));
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/AppleTree1.ply", scale));
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/AppleTree2.ply", scale));

            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/AppleTree0.ply", 2, scale));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/AppleTree1.ply", 2, scale));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/AppleTree2.ply", 2, scale));
        }

        public override CacheItem Type => CacheItem.AppleTrees;
    }
}