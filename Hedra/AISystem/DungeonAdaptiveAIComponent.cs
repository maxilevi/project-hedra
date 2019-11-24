using Hedra.AISystem.Behaviours;
using Hedra.Engine.EntitySystem;

namespace Hedra.AISystem
{
    public class DungeonAdaptiveAIComponent : AdaptiveAIComponent
    {
        public DungeonAdaptiveAIComponent(Entity Parent) : base(Parent)
        {
            AlterBehaviour<RoamBehaviour>(new DungeonRoamBehaviour(Parent));
        }
    }
}