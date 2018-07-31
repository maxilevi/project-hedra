using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public class BushesCache : CacheType
    {
        public BushesCache()
        {
            this.AddModel(AssetManager.PLYLoader("Assets/Env/Bush1.ply", Vector3.One));
            this.AddModel(AssetManager.PLYLoader("Assets/Env/Bush2.ply", Vector3.One));
            this.AddModel(AssetManager.PLYLoader("Assets/Env/Bush3.ply", Vector3.One));
        }
    }
}
