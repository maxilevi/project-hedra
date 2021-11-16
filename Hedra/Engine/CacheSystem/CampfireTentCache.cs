using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class CampfireTentCache : CacheType
    {
        public CampfireTentCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Campfire1.ply", Vector3.One));

            var tentShapes = AssetManager.LoadCollisionShapes("Campfire0.ply", 7, Vector3.One);
            tentShapes.RemoveAt(0);
            AddShapes(tentShapes);
        }

        public override CacheItem Type => CacheItem.CampfireTent;
    }
}