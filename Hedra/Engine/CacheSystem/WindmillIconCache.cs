using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class WindmillIconCache : CacheType
    {
        public override CacheItem Type => CacheItem.WindmillIcon;

        public WindmillIconCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Objects/Windmill.ply", Vector3.One));
        }
    }
}