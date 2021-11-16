using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class MushroomCache : CacheType
    {
        public MushroomCache()
        {
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Plants/Mushroom0.ply", Vector3.One));
        }

        public override CacheItem Type => CacheItem.Mushroom;
    }
}