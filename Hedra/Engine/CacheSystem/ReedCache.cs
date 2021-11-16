using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class ReedCache : CacheType
    {
        public ReedCache()
        {
            var model0 = AssetManager.PLYLoader("Assets/Env/Plants/Reed0.ply", Vector3.One);
            model0.AddWindValues();
            AddModel(model0);

            var model1 = AssetManager.PLYLoader("Assets/Env/Plants/Reed1.ply", Vector3.One);
            model1.AddWindValues();
            AddModel(model1);

            var model2 = AssetManager.PLYLoader("Assets/Env/Plants/Reed2.ply", Vector3.One);
            model2.AddWindValues();
            AddModel(model2);
        }

        public override CacheItem Type => CacheItem.Reed;
    }
}