using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class FishingPostIconCache : CacheType
    {
        public FishingPostIconCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Structures/FishingSettlement/FishingDock0-Icon.ply",
                Vector3.One));
        }

        public override CacheItem Type => CacheItem.FishingPostIcon;
    }
}