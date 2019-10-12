using Hedra.Engine.Management;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.CacheSystem
{
    class SleepingIconCache : CacheType
    {
        public SleepingIconCache()
        {
            this.AddModel(AssetManager.PLYLoader("Assets/Env/SleepingIcon.ply", Vector3.One));
        }
        
        public override CacheItem Type => CacheItem.SleepingIcon;
    }
}