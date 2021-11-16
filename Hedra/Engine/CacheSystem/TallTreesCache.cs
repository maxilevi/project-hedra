using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class TallTreesCache : CacheType
    {
        public TallTreesCache()
        {
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/TallTree0.ply", Vector3.One));
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/TallTree1.ply", Vector3.One));
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/TallTree2.ply", Vector3.One));

            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/TallTree0.ply", 2, Vector3.One));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/TallTree1.ply", 2, Vector3.One));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/TallTree2.ply", 2, Vector3.One));
        }

        public override CacheItem Type => CacheItem.TallTrees;
    }
}