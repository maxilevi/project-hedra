
using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    class CypressTreesCache : CacheType
    {
        public CypressTreesCache()
        {
            this.AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/Cypress0.ply", Vector3.One));
            this.AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/Cypress1.ply", Vector3.One));
            this.AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Trees/Cypress2.ply", Vector3.One));

            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/Cypress0.ply", 2, Vector3.One));
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/Cypress1.ply", 2, Vector3.One));
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Trees/Cypress2.ply", 2, Vector3.One));

        }
        
        public override CacheItem Type => CacheItem.CypressTrees;
    }
}