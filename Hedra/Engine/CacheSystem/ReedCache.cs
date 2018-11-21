using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public class ReedCache : CacheType
    {
        public ReedCache()
        {
            var model0 = AssetManager.PLYLoader("Assets/Env/Plants/Reed0.ply", Vector3.One);
            model0.Extradata.AddRange(model0.GenerateWindValues(-Vector4.One, 1f));
            this.AddModel(model0);
            
            var model1 = AssetManager.PLYLoader("Assets/Env/Plants/Reed1.ply", Vector3.One);
            model1.Extradata.AddRange(model1.GenerateWindValues(-Vector4.One, 1f));
            this.AddModel(model1);
            
            var model2 = AssetManager.PLYLoader("Assets/Env/Plants/Reed2.ply", Vector3.One);
            model2.Extradata.AddRange(model2.GenerateWindValues(-Vector4.One, 1f));
            this.AddModel(model2);
        }
        
        public override CacheItem Type => CacheItem.Reed;
    }
}