using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;

namespace Hedra.Engine.AISystem.Behaviours
{
    public class AttackBehaviour : Behaviour
    {
        protected FollowBehaviour Follow { get; }
        private readonly Timer _followTimer;
        public Entity Target { get; private set; }

        public AttackBehaviour(Entity Parent) : base(Parent)
        {
            Follow = new FollowBehaviour(Parent);
            _followTimer = new Timer(16);
        }

        public void SetTarget(Entity Target)
        {
            if(Target.IsDead) return;

            this.Target = Target;
            Follow.Target = this.Target;
            _followTimer.Reset();
        }

        public override void Update()
        {
            if (_followTimer.Tick() || !Follow.Enabled)
            {
                this.Target = null;
                Follow.Target = this.Target;
            }
            if (!Parent.Model.IsAttacking && Target != null && !Parent.InAttackRange(Target))
            {
                Follow.Update();
            }
            if (Target != null && Parent.InAttackRange(Target))
            {
                _followTimer.Reset();
                Parent.Model.Attack(Target);            
            }
        }

        public bool Enabled => this.Target != null;
    }
}
