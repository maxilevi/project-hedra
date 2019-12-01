using Hedra.Engine.Management;
using System.Numerics;

namespace Hedra.Engine.CacheSystem
{
    public class WitchHutIconCache : CacheType
    {
        public override CacheItem Type => CacheItem.WitchHutIcon;
        
        public WitchHutIconCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Structures/WitchHut/WitchHut0-Icon.ply", Vector3.One));
        }
    }
}