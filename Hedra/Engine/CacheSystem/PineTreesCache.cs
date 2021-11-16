using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    internal class PineTreesCache : CacheType
    {
        public PineTreesCache()
        {
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/Pine0.ply", Vector3.One));
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/Pine1.ply", Vector3.One));
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/Pine2.ply", Vector3.One));
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/Pine3.ply", Vector3.One));

            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/Pine0.ply", 2, Vector3.One));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/Pine1.ply", 3, Vector3.One));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/Pine2.ply", 2, Vector3.One));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/Pine3.ply", 2, Vector3.One));
        }

        public override CacheItem Type => CacheItem.PineTrees;
    }
}