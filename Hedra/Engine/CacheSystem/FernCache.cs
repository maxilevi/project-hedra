using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class FernCache : CacheType
    {
        public FernCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Fern.ply", Vector3.One));
        }

        public override CacheItem Type => CacheItem.Fern;
    }
}