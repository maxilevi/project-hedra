using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.ModuleSystem.AnimationEvents;
using Hedra.EntitySystem;
using System.Numerics;

namespace Hedra.AISystem.Behaviours
{
    public class ChargeBehaviour : Behaviour
    {
        private const int ChargeDistance = 48;
        private readonly Timer _chargeTimer;
        private IEntity _target { get; set; }
        protected WalkBehaviour Walk { get; }
        private bool _previousCollidesWithEntities;
        
        public ChargeBehaviour(IEntity Parent) : base(Parent)
        {
            Walk = new WalkBehaviour(Parent);
            _chargeTimer = new Timer(6 + Utils.Rng.Next(0, 4));
        }

        public override void Update()
        {
            base.Update();
            if (_target != null)
            {
                if (_chargeTimer.Tick() && IsInChargeRadius()) SetCharging(true);
                if (IsCharging) UpdateCharge();
            }
        }

        private void SetCharging(bool State)
        {
            IsCharging = State;
            if (IsCharging)
            {
                var direction = (_target.Position - Parent.Position).NormalizedFast();
                Walk.SetTarget(_target.Position + direction * ChargeDistance);
                Parent.AddBonusSpeedWhile(-Parent.Speed + Parent.Speed * 2.5f, () => IsCharging);
                EnableCollision();
            }
            else
            {
                DisableCollision();
            }
        }
        
        private void UpdateCharge()
        {
            _chargeTimer.Reset();
            if (_target == null || !Walk.HasTarget || Parent.IsStuck)
            {
                if (Parent.IsStuck)
                {
                    Parent.KnockForSeconds(5f);
                    Walk.Cancel();
                }

                SetCharging(false);
            }

            if (_target != null)
            {
                ChargeEffect.Do(Parent);
                if (Parent.InAttackRange(_target))
                {
                    if (!_target.IsKnocked) _target.KnockForSeconds(2);
                    _target.Damage(Parent.AttackDamage * Time.DeltaTime, Parent, out _, false);
                }
            }

            Walk.Update();
        }

        private void EnableCollision()
        {
            _previousCollidesWithEntities = Parent.Physics.CollidesWithStructures;
            Parent.Physics.CollidesWithStructures = true;
        }

        private void DisableCollision()
        {
            Parent.Physics.CollidesWithStructures = _previousCollidesWithEntities;
        }
        
        private bool IsInChargeRadius()
        {
            return (_target.Position.Xz() - Parent.Position.Xz()).LengthSquared() > ChargeDistance * ChargeDistance;
        }

        public void SetTarget(IEntity Target)
        {
            if(Target.IsDead) return;
            _target = Target;
        }
        
        public bool IsCharging { get; private set; }

        public override void Dispose()
        {
            Walk.Dispose();
        }
    }
}