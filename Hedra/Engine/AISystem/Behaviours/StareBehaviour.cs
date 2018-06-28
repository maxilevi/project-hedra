using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;

namespace Hedra.Engine.AISystem.Behaviours
{
    internal class StareBehaviour : Behaviour
    {
        public StareBehaviour(Entity Parent) : base(Parent)
        {
        }

        public override void Update()
        {
            var nearHumanoids = World.InRadius<Humanoid>(Parent.Position, 16f);
            if (nearHumanoids.Length > 0)
            {
                Physics.LookAt(Parent, nearHumanoids[0]);
            }
        }
    }
}