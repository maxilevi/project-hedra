using System.Linq;
using Hedra.Core;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Behaviours
{
    public class HostileBehaviour : Behaviour
    {
        protected float Radius { get; } = 64;
        protected AttackBehaviour Attack { get; }
        private readonly Timer _timer;

        public HostileBehaviour(IEntity Parent) : base(Parent)
        {
            Attack = new AttackBehaviour(Parent);
            _timer = new Timer(.5f);
        }

        protected virtual IEntity GetTarget()
        {
            return World.InRadius<IPlayer>(Parent.Position, Radius).FirstOrDefault(P => (Parent.Position - P.Position).LengthFast < Radius * P.Attributes.MobAggroModifier);
        }

        protected virtual void HandleTarget()
        {
            if (Attack.Target == null && _timer.Tick())
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
