using Hedra.AISystem.Behaviours;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Mob
{
    public class GroupHostileAIComponent : HostileAIComponent
    {
        public GroupHostileAIComponent(IEntity Parent) : base(Parent)
        {
            AlterBehaviour<HostileBehaviour>(new GroupHostileBehaviour(Parent));
        }
    }
}