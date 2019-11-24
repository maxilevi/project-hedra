using Hedra.AISystem.Behaviours;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Mob
{
    public class FoxAIComponent : HostileAIComponent
    {
        public FoxAIComponent(IEntity Parent) : base(Parent)
        {
            this.AlterBehaviour<HostileBehaviour>(new FoxHostileBehaviour(Parent));
        }
    }
}