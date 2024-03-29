using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class MerchantCartCache : CacheType
    {
        public MerchantCartCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/MerchantCart.ply", Vector3.One));

            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/MerchantCart.ply", 14, Vector3.One));
        }

        public override CacheItem Type => CacheItem.MerchantCart;
    }
}