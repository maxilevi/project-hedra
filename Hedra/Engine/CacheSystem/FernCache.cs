using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public class FernCache : CacheType
    {
        public FernCache()
        {
            this.AddModel(AssetManager.PLYLoader("Assets/Env/Fern.ply", Vector3.One));
        }
    }
}
