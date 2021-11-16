using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class WitchHutIconCache : CacheType
    {
        public WitchHutIconCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Structures/WitchHut/WitchHut0-Icon.ply", Vector3.One));
        }

        public override CacheItem Type => CacheItem.WitchHutIcon;
    }
}