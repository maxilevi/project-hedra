using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    internal class KnockedIconCache : CacheType
    {
        public KnockedIconCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/KnockedStars.ply", Vector3.One));
        }

        public override CacheItem Type => CacheItem.KnockedIcon;
    }
}