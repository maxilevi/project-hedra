using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class CampfireLogsCache : CacheType
    {
        public CampfireLogsCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Campfire2.ply", Vector3.One));

            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Campfire0.ply", 1, Vector3.One));
        }

        public override CacheItem Type => CacheItem.CampfireLogs;
    }
}