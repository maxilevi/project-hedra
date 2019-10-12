using Hedra.Engine.Management;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.CacheSystem
{
    public class DeadGrassCache : CacheType
    {
        public override CacheItem Type => CacheItem.DeadGrass;

        public DeadGrassCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Plants/DeadGrass0.ply", Vector3.One));
        }
    }
}