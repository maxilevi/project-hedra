using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    internal class CampfireRoasterCache : CacheType
    {
        public CampfireRoasterCache()
        {
            this.AddModel(AssetManager.PlyLoader("Assets/Env/Roaster.ply", Vector3.One));
            
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Roaster0.ply", 5, Vector3.One));
        }
    }
}