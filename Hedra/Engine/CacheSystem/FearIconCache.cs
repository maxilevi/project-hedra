using Hedra.Engine.Management;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.CacheSystem
{
    public class FearIconCache : CacheType
    {
        public FearIconCache()
        {
            this.AddModel(AssetManager.PLYLoader("Assets/Env/FearIcon.ply", Vector3.One));
        }
        
        public override CacheItem Type => CacheItem.FearIcon;
    }
}