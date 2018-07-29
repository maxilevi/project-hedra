using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    class KnockedIconCache : CacheType
    {
        public KnockedIconCache()
        {
            this.AddModel(AssetManager.PLYLoader("Assets/Env/KnockedStars.ply", Vector3.One));
        }
    }
}