using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class DeathIconCache : CacheType
    {
        public DeathIconCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/DeathIcon.ply", Vector3.One));
        }

        public override CacheItem Type => CacheItem.DeathIcon;
    }
}