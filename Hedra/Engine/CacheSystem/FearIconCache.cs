using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class FearIconCache : CacheType
    {
        public FearIconCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/FearIcon.ply", Vector3.One));
        }

        public override CacheItem Type => CacheItem.FearIcon;
    }
}