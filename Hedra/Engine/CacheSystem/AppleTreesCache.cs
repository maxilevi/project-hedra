using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public class AppleTreesCache : CacheType
    {
        public AppleTreesCache()
        {
            var scale = Vector3.One * 2;
            this.AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/AppleTree0.ply", scale));
            this.AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/AppleTree1.ply", scale));
            this.AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/AppleTree2.ply", scale));

            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/AppleTree0.ply", 2, scale));
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/AppleTree1.ply", 2, scale));
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/AppleTree2.ply", 2, scale));
        }

        public override CacheItem Type => CacheItem.AppleTrees;
    }
}