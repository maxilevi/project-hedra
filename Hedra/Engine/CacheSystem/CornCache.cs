using Hedra.Engine.Management;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.CacheSystem
{
    public class CornCache : CacheType
    {
        public override CacheItem Type => CacheItem.Corn;

        public CornCache()
        {
            var scale = Vector3.One * .65f;
            const float wind = .75f;
            var model0 = AssetManager.LoadPLYWithLODs("$DataFile$/Assets/Env/Plants/Corn0.ply", scale);
            AddModel(model0.AddWindValues(wind)); 
            AddModelPart(
                AssetManager.LoadPLYWithLODs("$DataFile$/Assets/Env/Plants/Corn0_Fruit0.ply", scale)
                    .AddWindValues(wind)
            );

            var model1 = AssetManager.LoadPLYWithLODs("$DataFile$/Assets/Env/Plants/Corn1.ply", scale);
            AddModel(model1.AddWindValues(wind));
            AddModelPart(
                AssetManager.LoadPLYWithLODs("$DataFile$/Assets/Env/Plants/Corn1_Fruit0.ply", scale)
                    .AddWindValues(wind)
            );
        }
    }
}