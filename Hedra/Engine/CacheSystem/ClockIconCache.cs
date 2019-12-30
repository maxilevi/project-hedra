using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class ClockIconCache : CacheType
    {
        public override CacheItem Type => CacheItem.ClockIcon;

        public ClockIconCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Objects/Clock.ply", Vector3.One));
        }
    }
}