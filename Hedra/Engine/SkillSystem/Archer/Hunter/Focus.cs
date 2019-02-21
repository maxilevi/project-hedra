using System;
using System.Globalization;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.WeaponSystem;

namespace Hedra.Engine.SkillSystem.Archer.Hunter
{
    public class Focus : PassiveSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Focus.png");
        private FocusComponent _component;
        
        protected override void Remove()
        {
            _component.StateUpdated -= StateUpdated;
            Player.RemoveComponent(_component);
        }

        protected override void Add()
        {
            Player.AddComponent(_component = new FocusComponent(Player, ChargeTime, DamageBonus));
            _component.StateUpdated += StateUpdated;
        }

        private void StateUpdated()
        {
            InvokeStateUpdated();
        }
        
        public override float IsAffectingModifier => _component?.IsAffectingModifier ?? 0;
        public override string Description => Translations.Get("focus_desc");
        public override string DisplayName => Translations.Get("focus_skill");
        protected override int MaxLevel => 15;
        private float ChargeTime => 10.5f - Level / 2f;
        private float DamageBonus => Math.Min(Level / 3f, 10 / 3f);
        
        private class FocusComponent : Component<IHumanoid>
        {
            public event OnStateUpdated StateUpdated;
            private readonly Timer _enemyTimer;
            private readonly Timer _betweenCharges;
            private readonly float _damageBonus;
            private bool _shouldCharge;
            
            public FocusComponent(IHumanoid Entity, float ChargeTime, float DamageBonus) : base(Entity)
            {
                _enemyTimer = new Timer(.1f);
                _damageBonus = DamageBonus;
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
                    _shouldCharge = !AreThereNearbyEnemies();
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
                if(IsAffectingModifier < 1) return;
                Options.DamageModifier *= (1 + _damageBonus);
                _betweenCharges.Reset();
            }
            
            private bool AreThereNearbyEnemies()
            {
                var entities = World.Entities;
                for (var i = 0; i < entities.Count; ++i)
                {
                    if((entities[i].Position - Parent.Position).Xz.LengthSquared > 114 * 114) continue;
                    if(entities[i].IsFriendly || entities[i] == Parent) continue;
                    return true;
                }
                return false;
            }

            public override void Dispose()
            {
                base.Dispose();
                Parent.BeforeAttack -= BeforeAttack;
            }

            public float IsAffectingModifier => _betweenCharges.Progress;
        }

        public override string[] Attributes => new[]
        {
            Translations.Get("focus_wait_change", ChargeTime.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("focus_damage_change", (int)(DamageBonus * 100)),
        };
    }
}