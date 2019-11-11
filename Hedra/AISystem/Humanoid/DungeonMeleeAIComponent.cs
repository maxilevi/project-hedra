using Hedra.AISystem.Behaviours;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Humanoid
{
    public class DungeonMeleeAIComponent : MeleeAIComponent
    {
        protected override bool CanExplore => false;
        public DungeonMeleeAIComponent(IHumanoid Parent, bool IsFriendly) : base(Parent, IsFriendly)
        {
            AlterBehaviour<TraverseBehaviour>(new DungeonTraverseBehaviour(Parent));
        }
    }
}