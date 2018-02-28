using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public class FlowerCache : CacheType
    {
        public FlowerCache()
        {
            this.AddModel(AssetManager.PlyLoader("Assets/Env/Tetrahedra.ply", Vector3.One));
        }
    }
}
