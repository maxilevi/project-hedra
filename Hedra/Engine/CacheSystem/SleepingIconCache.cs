using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    class SleepingIconCache : CacheType
    {
        public SleepingIconCache()
        {
            this.AddModel(AssetManager.PlyLoader("Assets/Env/SleepingIcon.ply", Vector3.One));
        }
    }
}