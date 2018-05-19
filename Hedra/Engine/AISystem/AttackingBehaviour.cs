using System;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;

namespace Hedra.Engine.AISystem
{
    public class AttackBehaviour : Behaviour
    {
        protected FollowBehaviour Follow { get; }
        private readonly Timer _followTimer;
        public Entity Target { get; private set; }

        public AttackBehaviour(Entity Parent) : base(Parent)
        {
            Follow = new FollowBehaviour(Parent);
            _followTimer = new Timer(16f);
        }

        public void SetTarget(Entity Target)
        {
            this.Target = Target;
            Follow.Target = this.Target;
            _followTimer.Reset();
        }

        public override void Update()
        {
            if (_followTimer.Tick())
            {
                this.Target = null;
                Follow.Target = this.Target;
            }
            if (!Parent.Model.IsAttacking)
            {
                Follow.Update();
            }

            if (Target != null && Parent.InAttackRange(Target))
            {
                _followTimer.Reset();
                Parent.Model.Attack(Target, Parent.AttackDamage + Utils.Rng.NextFloat() * 3 * 2f - 3f);            
            }
        }

        public bool Enabled => Follow.Enabled;
    }
}
