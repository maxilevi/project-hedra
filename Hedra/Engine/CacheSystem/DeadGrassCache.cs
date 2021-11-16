using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class DeadGrassCache : CacheType
    {
        public DeadGrassCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Plants/DeadGrass0.ply", Vector3.One));
        }

        public override CacheItem Type => CacheItem.DeadGrass;
    }
}