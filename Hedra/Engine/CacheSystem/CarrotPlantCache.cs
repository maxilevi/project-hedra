using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class CarrotPlantCache : CacheType
    {
        public CarrotPlantCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Plants/Carrot0.ply", Vector3.One * 1.25f));
        }

        public override CacheItem Type => CacheItem.Carrot;
    }
}