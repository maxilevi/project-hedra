using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    class CloudCache : CacheType
    {
        public CloudCache()
        {
            var model = AssetManager.PlyLoader("Assets/Env/Cloud0.ply", Vector3.One);
            model.ReColor(Vector4.One);
            model.FillExtraData(WorldRenderer.NoShadowsFlag);

            this.AddModel(model);
        }
    }
}