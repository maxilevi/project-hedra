using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    internal class ObeliskCache : CacheType
    {
        public ObeliskCache()
        {
            this.AddModel(AssetManager.PlyLoader("Assets/Env/Obelisk.ply", Vector3.One));
        }
    }
}