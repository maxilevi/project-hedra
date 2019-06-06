using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public class CabbageCache : CacheType
    {
        public override CacheItem Type => CacheItem.Cabbage;

        public CabbageCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Plants/Cabbage0.ply", Vector3.One));
            AddModel(AssetManager.PLYLoader("Assets/Env/Plants/Cabbage1.ply", Vector3.One));
        }
    }
}