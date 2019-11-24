using System.Linq;
using Hedra.Core;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using System.Numerics;
using Hedra.Engine.Core;
using Hedra.Numerics;

namespace Hedra.AISystem.Behaviours
{
    public class HostileBehaviour : Behaviour
    {
        protected virtual float Radius { get; } = 64;
        protected AttackBehaviour Attack { get; }
        private readonly Timer _timer;

        public HostileBehaviour(IEntity Parent) : base(Parent)
        {
            Attack = new AttackBehaviour(Parent);
            _timer = new Timer(.5f);
        }

        protected virtual IEntity GetTarget()
        {
            var target = GetTargets<IPlayer>().FirstOrDefault();
            if (target != null && Parent.Physics.StaticRaycast(target.Position + Vector3.UnitY * target.Model.Height * .5f))
                return null;
            return target;
        }

        protected T[] GetTargets<T>() where T : ISearchable, IEntity
        {
            return World.InRadius<T>(Parent.Position, Radius).Where(P => (Parent.Position - P.Position).LengthFast() < Radius * P.Attributes.MobAggroModifier).ToArray();
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

        public void Draw()
        {
            Attack.Draw();
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
