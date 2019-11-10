using Hedra.EntitySystem;

namespace Hedra.AISystem.Humanoid
{
    public class DungeonMageAIComponent : MageAIComponent
    {
        public DungeonMageAIComponent(IHumanoid Parent, bool IsFriendly) : base(Parent, IsFriendly)
        {
        }
    }
}