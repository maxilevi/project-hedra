using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    internal class GrassCache : CacheType
    {
        public GrassCache()
        {
            /*
             * WARNING: if you add more models here, equality operators on Chunk.cs won't work,
             *  fix that before adding new models
             */
            var model = AssetManager.PlyLoader("Assets/Env/Grass.ply", Vector3.One);
            model.ExtraData.AddRange(model.GenerateWindValues());
            for (int i = 0; i < model.ExtraData.Count; i++)
            {
                model.ExtraData[i] *= .45f;
            }

            this.AddModel(model);
        }
    }
}
