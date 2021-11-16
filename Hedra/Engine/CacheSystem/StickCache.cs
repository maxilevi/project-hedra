using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class StickCache : CacheType
    {
        public StickCache()
        {
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Plants/WoodenStick0.ply", Vector3.One));
        }

        public override CacheItem Type => CacheItem.Stick;
    }
}