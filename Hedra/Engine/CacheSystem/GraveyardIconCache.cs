using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class GraveyardIconCache : CacheType
    {
        public GraveyardIconCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Grave4.ply", Vector3.One * 2f));
        }

        public override CacheItem Type => CacheItem.GraveyardIcon;
    }
}