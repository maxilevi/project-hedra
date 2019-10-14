using System;
using System.Globalization;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Rendering;
using Hedra.WeaponSystem;

namespace Hedra.Engine.SkillSystem.Archer.Hunter
{
    public class Focus : ChargeablePassiveSkill
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Focus.png");

        protected override ChargeableComponent CreateComponent()
        {
            return new FocusComponent(User, ChargeTime, DamageBonus);
        }

        public override string Description => Translations.Get("focus_desc");
        public override string DisplayName => Translations.Get("focus_skill");
        protected override int MaxLevel => 15;
        private float ChargeTime => 10.5f - Level / 2f;
        private float DamageBonus => Math.Min(Level / 3f, 10 / 3f);
        
        private class FocusComponent : ChargeableComponent
        {
            private readonly float _damageBonus;
            
            public FocusComponent(IHumanoid Entity, float ChargeTime, float DamageBonus) : base(Entity, ChargeTime)
            {
                _damageBonus = DamageBonus;
            }

            protected override bool ShouldCharge()
            {
                return !AreThereNearbyEnemies();
            }

            protected override void Apply(AttackOptions Options)
            {
                Options.DamageModifier *= (1 + _damageBonus);
            }

            private bool AreThereNearbyEnemies()
            {
                var entities = World.Entities;
                for (var i = 0; i < entities.Count; ++i)
                {
                    if((entities[i].Position - Parent.Position).Xz().LengthSquared() > 114 * 114) continue;
                    if(entities[i].IsFriendly || entities[i] == Parent) continue;
                    return true;
                }
                return false;
            }
        }

        public override string[] Attributes => new[]
        {
            Translations.Get("focus_wait_change", ChargeTime.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("focus_damage_change", (int)(DamageBonus * 100)),
        };
    }
}