using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    internal class SleepingIconCache : CacheType
    {
        public SleepingIconCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/SleepingIcon.ply", Vector3.One));
        }

        public override CacheItem Type => CacheItem.SleepingIcon;
    }
}