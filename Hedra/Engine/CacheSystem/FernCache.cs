using Hedra.Engine.Management;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.CacheSystem
{
    public class FernCache : CacheType
    {
        public FernCache()
        {
            this.AddModel(AssetManager.PLYLoader("Assets/Env/Fern.ply", Vector3.One));
        }
        
        public override CacheItem Type => CacheItem.Fern;
    }
}
