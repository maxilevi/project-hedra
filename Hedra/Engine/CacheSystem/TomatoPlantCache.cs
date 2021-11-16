using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class TomatoPlantCache : CacheType
    {
        public TomatoPlantCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Plants/Tomato0.ply", Vector3.One * 1.75f));
            AddModel(AssetManager.PLYLoader("Assets/Env/Plants/Tomato1.ply", Vector3.One * 1.75f));
        }

        public override CacheItem Type => CacheItem.Tomato;
    }
}