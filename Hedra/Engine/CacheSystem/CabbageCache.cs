using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class CabbageCache : CacheType
    {
        public CabbageCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Plants/Cabbage0.ply", Vector3.One * 1.4f));
            AddModel(AssetManager.PLYLoader("Assets/Env/Plants/Cabbage1.ply", Vector3.One * 1.4f));
        }

        public override CacheItem Type => CacheItem.Cabbage;
    }
}