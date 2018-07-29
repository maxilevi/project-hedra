using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    internal class GraveyardIconCache : CacheType
    {
        public GraveyardIconCache()
        {
            this.AddModel(AssetManager.PLYLoader("Assets/Env/Grave4.ply", Vector3.One * 2f));
        }
    }
}
