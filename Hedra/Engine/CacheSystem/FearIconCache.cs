using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    internal class FearIconCache : CacheType
    {
        public FearIconCache()
        {
            this.AddModel(AssetManager.PLYLoader("Assets/Env/FearIcon.ply", Vector3.One));
        }
    }
}