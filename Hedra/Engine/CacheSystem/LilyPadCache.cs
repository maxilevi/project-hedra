using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class LilyPadCache : CacheType
    {
        public override CacheItem Type => CacheItem.LilyPad;

        public LilyPadCache()
        {
            var scale = Vector3.One * 5;
            //AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Plants/LilyPad0.ply", scale));
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Plants/LilyPad1.ply", scale));
        }
    }
}