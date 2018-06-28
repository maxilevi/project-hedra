using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    internal class BushesCache : CacheType
    {
        public BushesCache()
        {
            this.AddModel(AssetManager.PlyLoader("Assets/Env/Bush1.ply", Vector3.One));
            this.AddModel(AssetManager.PlyLoader("Assets/Env/Bush2.ply", Vector3.One));
            this.AddModel(AssetManager.PlyLoader("Assets/Env/Bush3.ply", Vector3.One));
        }
    }
}
