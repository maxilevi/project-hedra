using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class GarrisonCache : CacheType
    {
        public override CacheItem Type => CacheItem.Garrison;

        public GarrisonCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Structures/Garrison/Garrison0.ply", Vector3.One));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Structures/Garrison/Garrison0.ply", Vector3.One));
        }
    }
}