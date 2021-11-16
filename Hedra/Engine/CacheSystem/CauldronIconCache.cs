using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class CauldronIconCache : CacheType
    {
        public CauldronIconCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Objects/Cauldron.ply", Vector3.One));
        }

        public override CacheItem Type => CacheItem.CauldronIcon;
    }
}