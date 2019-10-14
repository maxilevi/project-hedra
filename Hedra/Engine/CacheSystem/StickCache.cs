using Hedra.Engine.Management;
using System.Numerics;

namespace Hedra.Engine.CacheSystem
{
    public class StickCache : CacheType
    {
        public override CacheItem Type => CacheItem.Stick;

        public StickCache()
        {
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Plants/WoodenStick0.ply", Vector3.One));
        }
    }
}