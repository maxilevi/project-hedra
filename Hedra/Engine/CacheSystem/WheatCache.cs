
using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    internal class WheatCache : CacheType
    {
        public WheatCache()
        {
            /*
             * WARNING: if you add more models here, equality operators on Chunk.cs won't work,
             *  fix that before adding new models
             */

            var model = AssetManager.PlyLoader("Assets/Env/Wheat.ply", Vector3.One);
            model.ExtraData.AddRange(model.GenerateWindValues());

            this.AddModel(model);
        }
    }
}
