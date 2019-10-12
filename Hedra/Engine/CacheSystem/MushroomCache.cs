using Hedra.Engine.Management;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.CacheSystem
{
    public class MushroomCache : CacheType
    {
        public override CacheItem Type => CacheItem.Mushroom;

        public MushroomCache()
        {
            base.AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Plants/Mushroom0.ply", Vector3.One));
        }
    }
}