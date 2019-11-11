using Hedra.AISystem.Behaviours;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Humanoid
{
    public class DungeonRangedAIComponent : RangedAIComponent
    {
        protected override bool CanExplore => false;
        public DungeonRangedAIComponent(IHumanoid Parent, bool IsFriendly) : base(Parent, IsFriendly)
        {
            AlterBehaviour<TraverseBehaviour>(new DungeonTraverseBehaviour(Parent));
        }
    }
}