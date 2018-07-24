using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    internal class MushroomCache : CacheType
    {
        public MushroomCache()
        {
            this.AddModel(AssetManager.PlyLoader("Assets/Env/Mushroom0.ply", Vector3.One));
        }
    }
}