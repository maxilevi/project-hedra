using System.Security.Cryptography;
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

        public virtual void SetTarget(IEntity NewTarget)
        {
            if(NewTarget.IsDead) return;

            Target = NewTarget;
            Follow.Target = NewTarget;
            FollowTimer.Reset();
        }

        public override void Update()
        {
            if (FollowTimer.Tick() || !Follow.Enabled)
            {
                this.Target = null;
                Follow.Target = this.Target;
            }
            if (!Parent.Model.IsAttacking && Target != null && !InAttackRange(Target, 1.15f))
            {
                Follow.Update();
            }
            if (Target != null && InAttackRange(Target, 1.15f))
            {
                FollowTimer.Reset();
                this.Attack(2.0f);
            }
        }

        protected virtual void Attack(float RangeModifier)
        {
            Parent.RotateTowards(Target);
            if (Parent.Model.CanAttack(Target, RangeModifier))
            {
                Parent.Model.Attack(Target, RangeModifier);
                //if (!Target.IsMoving && !Target.IsKnocked && Utils.Rng.Next(0, 6) == 1) Target.KnockForSeconds(1.5f);
            }
        }
        
        protected bool InAttackRange(IEntity Entity, float RangeModifier = 1f)
        {
            return Parent.InAttackRange(Entity, RangeModifier);
        }

        public bool Enabled => this.Target != null;

        public override void Dispose()
        {
            Follow.Dispose();
        }
    }
}
