using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class BushesCache : CacheType
    {
        public BushesCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Bush1.ply", Vector3.One));
            AddModel(AssetManager.PLYLoader("Assets/Env/Bush2.ply", Vector3.One));
            AddModel(AssetManager.PLYLoader("Assets/Env/Bush3.ply", Vector3.One));
        }

        public override CacheItem Type => CacheItem.Bushes;
    }
}