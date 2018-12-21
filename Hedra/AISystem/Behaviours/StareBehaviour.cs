using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.PhysicsSystem;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Behaviours
{
    public class StareBehaviour : Behaviour
    {
        public StareBehaviour(IEntity Parent) : base(Parent)
        {
        }

        public override void Update()
        {
            var nearHumanoids = World.InRadius<IHumanoid>(Parent.Position, 16f);
            if (nearHumanoids.Length > 0)
            {
                Parent.RotateTowards(nearHumanoids[0]);
            }
        }
    }
}