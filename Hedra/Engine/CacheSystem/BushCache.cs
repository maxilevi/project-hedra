using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public class BushCache : CacheType
    {
        public BushCache()
        {
            this.AddModel(AssetManager.PlyLoader("Assets/Env/Bush1.ply", Vector3.One));
            this.AddModel(AssetManager.PlyLoader("Assets/Env/Bush2.ply", Vector3.One));
            this.AddModel(AssetManager.PlyLoader("Assets/Env/Bush3.ply", Vector3.One));
        }
    }
}
