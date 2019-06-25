using Hedra.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.EntitySystem;
using Hedra.WeaponSystem;

namespace Hedra.Engine.SkillSystem
{
    public abstract class ChargeablePassiveSkill : PassiveSkill
    {
        private ChargeableComponent _component;

        protected override void Remove()
        {
            _component.StateUpdated -= StateUpdated;
            User.RemoveComponent(_component);
        }

        protected override void Add()
        {
            User.AddComponent(_component = CreateComponent());
            _component.StateUpdated += StateUpdated;
        }

        private void StateUpdated()
        {
            InvokeStateUpdated();
        }

        protected abstract ChargeableComponent CreateComponent();

        public override float IsAffectingModifier => _component?.IsAffectingModifier ?? 0;

        protected abstract class ChargeableComponent : Component<IHumanoid>
        {
            public event OnStateUpdated StateUpdated;
            private readonly Timer _enemyTimer;
            private readonly Timer _betweenCharges;
            private bool _shouldCharge;

            protected ChargeableComponent(IHumanoid Entity, float ChargeTime) : base(Entity)
            {
                _enemyTimer = new Timer(.05f);
                _betweenCharges = new Timer(ChargeTime)
                {
                    AutoReset = false
                };
                Parent.BeforeAttack += BeforeAttack;
            }

            public override void Update()
            {
                if (_enemyTimer.Tick())
                {
                    var previous = _shouldCharge;
                    _shouldCharge = ShouldCharge();
                    if (previous != _shouldCharge)
                    {
                        _betweenCharges.Tick();
                        StateUpdated?.Invoke();
                    }
                }

                if (_shouldCharge)
                {
                    _betweenCharges.Tick();
                }
            }

            private void BeforeAttack(AttackOptions Options)
            {
                if (IsAffectingModifier < 1) return;
                Apply(Options);
                _betweenCharges.Reset();
            }

            protected abstract void Apply(AttackOptions Options);
            protected abstract bool ShouldCharge();

            public override void Dispose()
            {
                base.Dispose();
                Parent.BeforeAttack -= BeforeAttack;
            }

            public float IsAffectingModifier => _betweenCharges.Progress;
        }
    }
}