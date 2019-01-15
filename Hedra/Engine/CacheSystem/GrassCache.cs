using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public class GrassCache : CacheType
    {
        public GrassCache()
        {
            /*
             * WARNING: if you add more models here, equality operators on Chunk.cs won't work,
             *  fix that before adding new models
             */
            var model = AssetManager.PLYLoader("Assets/Env/Grass.ply", Vector3.One);
            model.AddWindValues(.45f);

            this.AddModel(model);
        }
        
        public override CacheItem Type => CacheItem.Grass;
    }
}
