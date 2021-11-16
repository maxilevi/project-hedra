using System.Numerics;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.CacheSystem
{
    internal class CloudCache : CacheType
    {
        public CloudCache()
        {
            var model = AssetManager.PLYLoader("Assets/Env/Cloud0.ply", Vector3.One);
            model.Paint(Vector4.One);
            model.FillExtraData(WorldRenderer.NoShadowsFlag);

            AddModel(model);
        }

        public override CacheItem Type => CacheItem.Cloud;
    }
}