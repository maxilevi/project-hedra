using Hedra.Engine.Management;
using System.Numerics;

namespace Hedra.Engine.CacheSystem
{
    public class MerchantIconCache : CacheType
    {
        public MerchantIconCache()
        {
            this.AddModel(AssetManager.PLYLoader("Assets/UI/MerchantIcon.ply", Vector3.One));
        }
        
        public override CacheItem Type => CacheItem.MerchantIcon;
    }
}
