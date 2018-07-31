using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    internal class MushroomCache : CacheType
    {
        public MushroomCache()
        {
            this.AddModel(AssetManager.PLYLoader("Assets/Env/Mushroom0.ply", Vector3.One));
            this.AddModel(AssetManager.PLYLoader("Assets/Env/Mushroom1.ply", Vector3.One));
        }
    }
}