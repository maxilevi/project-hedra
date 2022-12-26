
using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class MushroomTreesCache : CacheType
    {
        public MushroomTreesCache()
        {
            var scale = Vector3.One * 3.25f;
            for (var i = 0; i < 4; ++i)
            {
                AddModel(AssetManager.PLYLoader($"Assets/Env/Trees/MushroomTree{i}.ply", scale));
                AddShapes(AssetManager.LoadCollisionShapes($"Assets/Env/Trees/MushroomTree{i}.ply", scale));
            }
        }

        public override CacheItem Type => CacheItem.MushroomTrees;
    }
}