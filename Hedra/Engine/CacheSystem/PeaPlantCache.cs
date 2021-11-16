using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class PeaPlantCache : CacheType
    {
        public PeaPlantCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Plants/Peas0.ply", Vector3.One));
        }

        public override CacheItem Type => CacheItem.Peas;
    }
}