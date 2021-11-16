using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class OakTreesCache : CacheType
    {
        public OakTreesCache()
        {
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/Oak0.ply", Vector3.One));
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/Oak1.ply", Vector3.One));
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/Oak2.ply", Vector3.One));

            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/Oak0.ply", 10, Vector3.One));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/Oak1.ply", 2, Vector3.One));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/Oak2.ply", 6, Vector3.One));
        }

        public override CacheItem Type => CacheItem.OakTrees;
    }
}