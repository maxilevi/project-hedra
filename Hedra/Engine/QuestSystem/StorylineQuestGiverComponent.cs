using Hedra.Engine.CacheSystem;
using Hedra.EntitySystem;
using Hedra.Mission;

namespace Hedra.Engine.QuestSystem
{
    public class StorylineQuestGiverComponent : QuestGiverComponent
    {
        public StorylineQuestGiverComponent(IHumanoid Parent, IMissionDesign QuestArchetype) : base(Parent,
            QuestArchetype)
        {
        }

        protected override CacheItem AlertIcon => CacheItem.StorylineIcon;
    }
}