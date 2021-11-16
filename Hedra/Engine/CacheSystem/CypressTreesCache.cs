using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class CypressTreesCache : CacheType
    {
        public CypressTreesCache()
        {
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/Cypress0.ply", Vector3.One * 1.5f));
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/Cypress1.ply", Vector3.One * 1.5f));
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/Cypress2.ply", Vector3.One * 1.5f));

            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/Cypress0.ply", 2, Vector3.One));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/Cypress1.ply", 2, Vector3.One));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/Cypress2.ply", 2, Vector3.One));
        }

        public override CacheItem Type => CacheItem.CypressTrees;
    }
}