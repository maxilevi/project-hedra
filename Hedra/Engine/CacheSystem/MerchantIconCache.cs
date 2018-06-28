using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    internal class MerchantIconCache : CacheType
    {
        public MerchantIconCache()
        {
            this.AddModel(AssetManager.PlyLoader("Assets/UI/MerchantIcon.ply", Vector3.One));
        }
    }
}
