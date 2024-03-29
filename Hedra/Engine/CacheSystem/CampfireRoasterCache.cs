using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class CampfireRoasterCache : CacheType
    {
        public CampfireRoasterCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Roaster.ply", Vector3.One));

            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Roaster0.ply", 5, Vector3.One));
        }

        public override CacheItem Type => CacheItem.CampfireRoaster;
    }
}