using System.Linq;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Player;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Behaviours
{
    public class HostileBehaviour : Behaviour
    {
        protected float Radius { get; } = 64;
        protected AttackBehaviour Attack { get; }

        public HostileBehaviour(IEntity Parent) : base(Parent)
        {
            Attack = new AttackBehaviour(Parent);
        }

        protected virtual IEntity GetTarget()
        {
            return World.InRadius<IPlayer>(Parent.Position, Radius).FirstOrDefault();
        }

        protected virtual void HandleTarget()
        {
            if (Attack.Target == null)
            {
                var target = GetTarget();
                if (target != null)
                {
                    Attack.SetTarget(target);
                }
            }
        }
        
        public override void Update()
        {
            HandleTarget();
            Attack.Update();   
        }

        public bool Enabled => Attack.Enabled;

        public override void Dispose()
        {
            Attack.Dispose();
        }
    }
}
