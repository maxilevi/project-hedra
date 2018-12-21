using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Behaviours
{
    public class AttackBehaviour : Behaviour
    {
        protected FollowBehaviour Follow { get; }
        protected readonly Timer FollowTimer;
        public IEntity Target { get; protected set; }

        public AttackBehaviour(IEntity Parent) : base(Parent)
        {
            Follow = new FollowBehaviour(Parent);
            FollowTimer = new Timer(16);
        }

        public virtual void SetTarget(IEntity Target)
        {
            if(Target.IsDead) return;

            this.Target = Target;
            Follow.Target = this.Target;
            FollowTimer.Reset();
        }

        public override void Update()
        {
            if (FollowTimer.Tick() || !Follow.Enabled)
            {
                this.Target = null;
                Follow.Target = this.Target;
            }
            if (!Parent.Model.IsAttacking && Target != null && !InAttackRange(Target))
            {
                Follow.Update();
            }
            if (Target != null && InAttackRange(Target))
            {
                FollowTimer.Reset();
                this.Attack(2.0f);
            }
        }

        protected virtual void Attack(float RangeModifier)
        {
            Parent.RotateTowards(Target);
            Parent.Model.Attack(Target, RangeModifier);
        }
        
        protected bool InAttackRange(IEntity Entity)
        {
            return Parent.InAttackRange(Entity);
        }

        public bool Enabled => this.Target != null;
    }
}
