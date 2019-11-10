using Hedra.EntitySystem;

namespace Hedra.AISystem.Humanoid
{
    public class DungeonRangedAIComponent : RangedAIComponent
    {
        public DungeonRangedAIComponent(IHumanoid Parent, bool IsFriendly) : base(Parent, IsFriendly)
        {
        }
    }
}