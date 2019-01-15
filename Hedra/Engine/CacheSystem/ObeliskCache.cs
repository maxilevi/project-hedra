using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public class ObeliskCache : CacheType
    {
        public ObeliskCache()
        {
            this.AddModel(AssetManager.PLYLoader("Assets/Env/Obelisk.ply", Vector3.One));
        }
        
        public override CacheItem Type => CacheItem.Obelisk;
    }
}