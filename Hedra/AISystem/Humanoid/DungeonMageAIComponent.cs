using Hedra.AISystem.Behaviours;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Humanoid
{
    public class DungeonMageAIComponent : MageAIComponent
    {
        protected override bool CanExplore => false;
        public DungeonMageAIComponent(IHumanoid Parent, bool IsFriendly) : base(Parent, IsFriendly)
        {
            AlterBehaviour<TraverseBehaviour>(new DungeonTraverseBehaviour(Parent));
        }
    }
}