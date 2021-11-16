using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class BossIconCache : CacheType
    {
        public BossIconCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/QuestIcon.ply", Vector3.One * 6f, -Vector3.UnitY,
                Vector3.Zero));
        }

        public override CacheItem Type => CacheItem.BossIcon;
    }
}