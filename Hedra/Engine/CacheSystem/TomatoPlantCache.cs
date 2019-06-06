using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public class TomatoPlantCache : CacheType
    {
        public override CacheItem Type => CacheItem.Tomato;

        public TomatoPlantCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Plants/Tomato0.ply", Vector3.One));
            AddModel(AssetManager.PLYLoader("Assets/Env/Plants/Tomato1.ply", Vector3.One));
        }
    }
}