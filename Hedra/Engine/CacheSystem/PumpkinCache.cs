using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public class PumpkinCache : CacheType
    {
        public override CacheItem Type => CacheItem.Pumpkin;

        public PumpkinCache()
        {
            var scale = Vector3.One * .5f;
            const float wind = .5f;
            AddModel(AssetManager.PLYLoader("Assets/Env/Plants/Pumpkin0.ply", scale).AddWindValues(wind)); 
            AddModelPart(AssetManager.PLYLoader("Assets/Env/Plants/Pumpkin0_Fruit0.ply", scale).AddWindValues(wind));

            AddModel(AssetManager.PLYLoader("Assets/Env/Plants/Pumpkin1.ply", scale).AddWindValues(wind));
            AddModelPart(AssetManager.PLYLoader("Assets/Env/Plants/Pumpkin1_Fruit0.ply", scale).AddWindValues(wind));
        }
    }
}