using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;

namespace Hedra.Engine.AISystem.Behaviours
{
    public class AttackBehaviour : Behaviour
    {
        protected FollowBehaviour Follow { get; }
        protected readonly Timer FollowTimer;
        public Entity Target { get; protected set; }

        public AttackBehaviour(Entity Parent) : base(Parent)
        {
            Follow = new FollowBehaviour(Parent);
            FollowTimer = new Timer(16);
        }

        public void SetTarget(Entity Target)
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
            if (!Parent.Model.IsAttacking && Target != null && !Parent.InAttackRange(Target, 5f))
            {
                Follow.Update();
            }
            if (Target != null && Parent.InAttackRange(Target, 2.0f))
            {
                FollowTimer.Reset();
                this.Attack();
            }
        }

        public virtual void Attack()
        {
            Parent.Model.Attack(Target);
        }

        public bool Enabled => this.Target != null;
    }
}
