using Hedra.Engine.Management;
using System.Numerics;

namespace Hedra.Engine.CacheSystem
{
    public class GraveyardIconCache : CacheType
    {
        public GraveyardIconCache()
        {
            this.AddModel(AssetManager.PLYLoader("Assets/Env/Grave4.ply", Vector3.One * 2f));
        }
        
        public override CacheItem Type => CacheItem.GraveyardIcon;
    }
}
