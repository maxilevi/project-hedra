using System.Linq;
using System.Numerics;
using Hedra.Core;
using Hedra.Engine.Core;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.Numerics;

namespace Hedra.AISystem.Behaviours
{
    public class HostileBehaviour : Behaviour
    {
        private readonly Timer _timer;

        public HostileBehaviour(IEntity Parent) : base(Parent)
        {
            Attack = new AttackBehaviour(Parent);
            _timer = new Timer(.5f);
        }

        protected virtual float Radius { get; } = 64;
        protected AttackBehaviour Attack { get; }

        public bool Enabled => Attack.Enabled;

        protected virtual IEntity GetTarget()
        {
            var target = GetTargets<IPlayer>().FirstOrDefault();
            if (target != null && Parent.Physics.StaticRaycast(target.Position + Vector3.UnitY * target.Model.Height))
                return null;
            return target;
        }

        protected T[] GetTargets<T>() where T : ISearchable, IEntity
        {
            return World.InRadius<T>(Parent.Position, Radius).Where(P =>
                (Parent.Position - P.Position).LengthFast() < Radius * P.Attributes.MobAggroModifier).ToArray();
        }

        protected virtual void HandleTarget()
        {
            if (Attack.Target == null && _timer.Tick())
            {
                var target = GetTarget();
                if (target != null) SetTarget(target);
            }
        }

        public void SetTarget(IEntity Entity)
        {
            Attack.SetTarget(Entity);
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

        public override void Dispose()
        {
            Attack.Dispose();
        }
    }
}