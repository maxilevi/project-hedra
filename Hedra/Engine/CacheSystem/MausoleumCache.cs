using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    internal class MausoleumCache : CacheType
    {
        public MausoleumCache()
        {
            this.AddModel(AssetManager.PLYLoader("Assets/Env/Mausoleum.ply", Vector3.One));
            
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Mausoleum.ply", 2, Vector3.One));
        }
    }
}