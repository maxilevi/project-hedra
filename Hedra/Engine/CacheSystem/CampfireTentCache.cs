using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public class CampfireTentCache : CacheType
    {
        public CampfireTentCache()
        {
            this.AddModel(AssetManager.PLYLoader("Assets/Env/Campfire1.ply", Vector3.One));
            
            var tentShapes = AssetManager.LoadCollisionShapes("Campfire0.ply", 7, Vector3.One);
            tentShapes.RemoveAt(0);
            this.AddShapes(tentShapes);
        }
    }
}