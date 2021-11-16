using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class StorylineCache : CacheType
    {
        public StorylineCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/QuestIcon.ply", Vector3.One * 6f, -Vector3.UnitY,
                Vector3.Zero));
        }

        public override CacheItem Type => CacheItem.StorylineIcon;
    }
}