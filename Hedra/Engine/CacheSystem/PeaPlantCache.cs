using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public class PeaPlantCache : CacheType
    {
        public override CacheItem Type => CacheItem.Peas;

        public PeaPlantCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Plants/Peas0.ply", Vector3.One));
        }
    }
}