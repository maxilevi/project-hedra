using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;

namespace Hedra.Engine.AISystem.Behaviours
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

        public void SetTarget(IEntity Target)
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
            if (!Parent.Model.IsAttacking && Target != null && !Parent.InAttackRange(Target, 1.25f))
            {
                Follow.Update();
            }
            if (Target != null && Parent.InAttackRange(Target, 1.5f))
            {
                FollowTimer.Reset();
                this.Attack(1.5f);
            }
        }

        public virtual void Attack(float RangeModifier)
        {
            Physics.LookAt(this.Parent, Target);
            Parent.Model.Attack(Target, RangeModifier);
        }

        public bool Enabled => this.Target != null;
    }
}
