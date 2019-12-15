using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class CauldronIconCache : CacheType
    {
        public override CacheItem Type => CacheItem.CauldronIcon;

        public CauldronIconCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Objects/Cauldron.ply", Vector3.One));
        }
    }
}