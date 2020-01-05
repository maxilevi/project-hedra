using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class ThymeCache : CacheType
    {
        public override CacheItem Type => CacheItem.Thyme;

        public ThymeCache()
        {
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Plants/Thyme0.ply", Vector3.One));
        }
    }
}