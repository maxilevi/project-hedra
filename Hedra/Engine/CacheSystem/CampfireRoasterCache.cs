using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public class CampfireRoasterCache : CacheType
    {
        public CampfireRoasterCache()
        {
            this.AddModel(AssetManager.PLYLoader("Assets/Env/Roaster.ply", Vector3.One));
            
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Roaster0.ply", 5, Vector3.One));
        }
    }
}