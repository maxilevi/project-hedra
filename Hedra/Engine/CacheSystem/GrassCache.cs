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
            model.Extradata.AddRange(model.GenerateWindValues());
            for (int i = 0; i < model.Extradata.Count; i++)
            {
                model.Extradata[i] *= .45f;
            }

            this.AddModel(model);
        }
    }
}
