using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public class MerchantIconCache : CacheType
    {
        public MerchantIconCache()
        {
            this.AddModel(AssetManager.PlyLoader("Assets/UI/MerchantIcon.ply", Vector3.One));
        }
    }
}
