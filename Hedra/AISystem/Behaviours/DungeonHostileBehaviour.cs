using Hedra.EntitySystem;

namespace Hedra.AISystem.Behaviours
{
    public class DungeonHostileBehaviour : HostileBehaviour
    {
        public DungeonHostileBehaviour(IEntity Parent) : base(Parent)
        {
        }

        protected override float Radius => 48;
    }
}