using Hedra.Engine.Management;
using System.Numerics;

namespace Hedra.Engine.CacheSystem
{
    public class PumpkinCache : CacheType
    {
        public override CacheItem Type => CacheItem.Pumpkin;

        public PumpkinCache()
        {
            var scale = Vector3.One * .5f;
            const float wind = .5f;
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Plants/Pumpkin0.ply", scale).AddWindValues(wind)); 
            AddModelPart(AssetManager.LoadPLYWithLODs("Assets/Env/Plants/Pumpkin0_Fruit0.ply", scale));
            //AddModelPart(AssetManager.LoadPLYWithLODs("Assets/Env/Plants/Pumpkin0_Fruit1.ply", scale).AddWindValues(0));

            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Plants/Pumpkin1.ply", scale).AddWindValues(wind));
            AddModelPart(AssetManager.LoadPLYWithLODs("Assets/Env/Plants/Pumpkin1_Fruit0.ply", scale));
        }
    }
}