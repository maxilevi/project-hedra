using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class SunflowerCache : CacheType
    {
        public SunflowerCache()
        {
            var scale = Vector3.One * .75f;
            const float wind = .75f;
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Plants/Sunflower0.ply", scale).AddWindValues(wind));
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Plants/Sunflower1.ply", scale).AddWindValues(wind));
        }

        public override CacheItem Type => CacheItem.Sunflower;
    }
}