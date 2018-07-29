using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    internal class CampfireLogsCache : CacheType
    {
        public CampfireLogsCache()
        {
            this.AddModel(AssetManager.PLYLoader("Assets/Env/Campfire2.ply", Vector3.One));
            
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Campfire0.ply", 1, Vector3.One));
        }
    }
}