using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.ModuleSystem.AnimationEvents;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Behaviours
{
    public class ChargeBehaviour : Behaviour
    {
        private readonly Timer _chargeTimer;
        private IEntity _target { get; set; }
        protected WalkBehaviour Walk { get; }
        
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
                var toTarget = (_target.Position - Parent.Position);
                Walk.SetTarget(_target.Position + toTarget);
                Parent.AddBonusSpeedWhile(Parent.Speed * 3, () => IsCharging);
            }
        }
        
        private void UpdateCharge()
        {
            _chargeTimer.Reset();
            if (!Walk.HasTarget || _target == null)
            {
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

        private bool IsInChargeRadius()
        {
            return (_target.Position.Xz - Parent.Position.Xz).LengthSquared > 32 * 32;
        }

        public void SetTarget(IEntity Target)
        {
            if(Target.IsDead) return;
            _target = Target;
        }
        
        public bool IsCharging { get; private set; }
    }
}