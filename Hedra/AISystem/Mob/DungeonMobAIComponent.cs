using Hedra.AISystem.Behaviours;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Mob
{
    public class DungeonMobAIComponent : HostileAIComponent
    {
        public DungeonMobAIComponent(IEntity Parent) : base(Parent)
        {
            AlterBehaviour<TraverseBehaviour>(new DungeonTraverseBehaviour(Parent));
        }
    }
}