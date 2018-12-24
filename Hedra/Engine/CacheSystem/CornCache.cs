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
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Plants/Corn0.ply", scale).AddWindValues(wind)); 
            AddModelPart(AssetManager.LoadPLYWithLODs("Assets/Env/Plants/Corn0_Fruit0.ply", scale).AddWindValues(wind));

            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Plants/Corn1.ply", scale).AddWindValues(wind));
            AddModelPart(AssetManager.LoadPLYWithLODs("Assets/Env/Plants/Corn1_Fruit0.ply", scale).AddWindValues(wind));
        }
    }
}