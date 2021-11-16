using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class MerchantIconCache : CacheType
    {
        public MerchantIconCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/UI/MerchantIcon.ply", Vector3.One));
        }

        public override CacheItem Type => CacheItem.MerchantIcon;
    }
}