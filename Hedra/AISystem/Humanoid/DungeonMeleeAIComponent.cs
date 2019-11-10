using Hedra.EntitySystem;

namespace Hedra.AISystem.Humanoid
{
    public class DungeonMeleeAIComponent : MeleeAIComponent
    {
        public DungeonMeleeAIComponent(IHumanoid Parent, bool IsFriendly) : base(Parent, IsFriendly)
        {
        }
    }
}