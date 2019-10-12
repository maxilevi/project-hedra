using Hedra.Engine.Management;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.CacheSystem
{
    public class CampfireCache : CacheType
    {
        public CampfireCache()
        {
            this.AddModel(AssetManager.PLYLoader("Assets/Env/Campfire0.ply", Vector3.One));
            
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Campfire0.ply", 7, Vector3.One));
        }
        
        public override CacheItem Type => CacheItem.Campfire;
    }
}