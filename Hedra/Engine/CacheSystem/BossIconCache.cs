using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    internal class BossIconCache : CacheType
    {
        public BossIconCache()
        {
            this.AddModel(AssetManager.PLYLoader("Assets/Env/QuestIcon.ply", Vector3.One * 6f, -Vector3.UnitY, Vector3.Zero));
        }
    }
}
