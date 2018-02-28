
using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    class FarmCache : CacheType
    {
        public FarmCache()
        {
            this.AddModel(AssetManager.PlyLoader("Assets/Env/Village/Farm0.ply", Vector3.One));
            this.AddModel(AssetManager.PlyLoader("Assets/Env/Village/Farm1.ply", Vector3.One));
        }
    }
}
