using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class LilyPadCache : CacheType
    {
        public override CacheItem Type => CacheItem.LilyPad;

        public LilyPadCache()
        {
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Plants/LilyPad0.ply", Vector3.One));
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Plants/LilyPad1.ply", Vector3.One));
        }
    }
}