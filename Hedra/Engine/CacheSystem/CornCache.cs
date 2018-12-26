using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public class CornCache : CacheType
    {
        public override CacheItem Type => CacheItem.Corn;

        public CornCache()
        {
            var scale = Vector3.One * .65f;
            const float wind = .75f;
            var model0 = AssetManager.LoadPLYWithLODs("Assets/Env/Plants/Corn0.ply", scale);
            AddModel(model0.AddWindValues(wind)); 
            AddModelPart(
                AssetManager.LoadPLYWithLODs("Assets/Env/Plants/Corn0_Fruit0.ply", scale)
                    .AddWindValues(/*
                        model0.SupportPoint(-Vector3.UnitY),
                        model0.SupportPoint(Vector3.UnitY),*/
                        wind
                    )
            );

            var model1 = AssetManager.LoadPLYWithLODs("Assets/Env/Plants/Corn1.ply", scale);
            AddModel(model1.AddWindValues(wind));
            AddModelPart(
                AssetManager.LoadPLYWithLODs("Assets/Env/Plants/Corn1_Fruit0.ply", scale)
                    .AddWindValues(/*
                        model1.SupportPoint(-Vector3.UnitY),
                        model1.SupportPoint(Vector3.UnitY),*/
                        wind
                    )
            );
        }
    }
}