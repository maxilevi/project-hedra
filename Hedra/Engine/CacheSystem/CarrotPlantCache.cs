using Hedra.Engine.Management;
using System.Numerics;

namespace Hedra.Engine.CacheSystem
{
    public class CarrotPlantCache : CacheType
    {
        public override CacheItem Type => CacheItem.Carrot;

        public CarrotPlantCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Plants/Carrot0.ply", Vector3.One * 1.25f));
        }
    }
}