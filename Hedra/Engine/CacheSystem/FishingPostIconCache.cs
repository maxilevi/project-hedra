using Hedra.Engine.Management;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.CacheSystem
{
    public class FishingPostIconCache : CacheType
    {
        public override CacheItem Type => CacheItem.FishingPostIcon;

        public FishingPostIconCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Structures/FishingSettlement/FishingDock0-Icon.ply", Vector3.One));
        }
    }
}