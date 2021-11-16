using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class CampfireCache : CacheType
    {
        public CampfireCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Campfire0.ply", Vector3.One));

            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Campfire0.ply", 7, Vector3.One));
        }

        public override CacheItem Type => CacheItem.Campfire;
    }
}