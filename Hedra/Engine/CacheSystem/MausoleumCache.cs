using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class MausoleumCache : CacheType
    {
        public MausoleumCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Mausoleum.ply", Vector3.One));

            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Mausoleum.ply", 2, Vector3.One));
        }

        public override CacheItem Type => CacheItem.Mausoleum;
    }
}