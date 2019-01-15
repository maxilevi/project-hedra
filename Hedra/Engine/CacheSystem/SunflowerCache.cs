using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public class SunflowerCache : CacheType
    {
        public override CacheItem Type => CacheItem.Sunflower;
        
        public SunflowerCache()
        {
            var scale = Vector3.One * .75f;
            const float wind = .75f;
            AddModel(AssetManager.PLYLoader("Assets/Env/Plants/Sunflower0.ply", scale).AddWindValues(wind)); 
            AddModel(AssetManager.PLYLoader("Assets/Env/Plants/Sunflower1.ply", scale).AddWindValues(wind));
        }
    }
}