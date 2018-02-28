using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    class CampfireCache : CacheType
    {
        public CampfireCache()
        {
            this.AddModel(AssetManager.PlyLoader("Assets/Env/Campfire0.ply", Vector3.One));
        }
    }
}