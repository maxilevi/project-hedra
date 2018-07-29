using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    internal class MerchantCartCache : CacheType
    {
        public MerchantCartCache()
        {
            this.AddModel(AssetManager.PLYLoader("Assets/Env/MerchantCart.ply", Vector3.One));
            
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/MerchantCart.ply", 14, Vector3.One));
        }
    }
}