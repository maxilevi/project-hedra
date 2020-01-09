using System.Security.Cryptography;
using Hedra.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using System.Numerics;

namespace Hedra.AISystem.Behaviours
{
    public delegate void TargetChangedEvent(IEntity Target);
    
    public class AttackBehaviour : Behaviour
    {
        public event TargetChangedEvent TargetChanged;
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
            TargetChanged?.Invoke(NewTarget);
        }

        public override void Update()
        {
            HandleFollowing();
            if (Target != null)
            {
                var ride = Target.SearchComponent<Hedra.Components.RideComponent>();
                if (ride?.Rider != null)
                {
                    SetTarget(ride.Rider);
                }
            }
            if (!Parent.Model.IsAttacking && Target != null && !InAttackRange(Target, 1.35f))
            {
                Follow.Update();
            }
            if (Target != null && InAttackRange(Target, 1.35f))
            {
                FollowTimer.Reset();
                this.Attack(2.0f);
            }
        }

        public void Draw()
        {
            if(Target == null) return;
            BasicGeometry.DrawLine(Parent.Position + Vector3.UnitY, Target.Position + Vector3.UnitY, Vector4.One, 2);
            BasicGeometry.DrawPoint(Target.Position, Vector4.One);
        }

        protected void HandleFollowing()
        {
            if (!FollowTimer.Tick() && Follow.Enabled) return;
            ResetTarget();
        }

        protected virtual void Attack(float RangeModifier)
        {
            Parent.LookAt(Target);
            if (Parent.Model.CanAttack(Target, RangeModifier))
            {
                Parent.Model.Attack(Target, RangeModifier);
                //if (!Target.IsMoving && !Target.IsKnocked && Utils.Rng.Next(0, 6) == 1) Target.KnockForSeconds(1.5f);
            }
        }

        public void ResetTarget()
        {
            Target = null;
            Follow.Target = Target;
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
