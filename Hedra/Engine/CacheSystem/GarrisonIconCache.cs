using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class GarrisonIconCache : CacheType
    {
        public GarrisonIconCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Structures/Garrison/Garrison0-Icon.ply", Vector3.One * .5f));
        }

        public override CacheItem Type => CacheItem.GarrisonIcon;
    }
}