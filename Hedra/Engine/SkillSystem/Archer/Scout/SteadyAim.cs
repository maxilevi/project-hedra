using System;
using System.Globalization;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Rendering;
using Hedra.WeaponSystem;
using Hedra.WorldObjects;

namespace Hedra.Engine.SkillSystem.Archer.Scout
{
    public class SteadyAim : ChargeablePassiveSkill
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/SteadyAim.png");
        protected override bool ShouldDisable => !User.HasWeapon || !(User.LeftWeapon is Bow);

        public override string Description => Translations.Get("steady_aim_desc");
        public override string DisplayName => Translations.Get("steady_aim");
        protected override int MaxLevel => 15;
        private float ChargeTime => 7.0f - Level / 3f;
        private float PrecisionBonus => Math.Max(0, 1 - Level / 8f);

        public override string[] Attributes => new[]
        {
            Translations.Get("steady_wait_change", ChargeTime.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("steady_precision_change", (int)((1 - PrecisionBonus) * 100))
        };

        protected override ChargeableComponent CreateComponent()
        {
            return new SteadyAimComponent(User, ChargeTime, PrecisionBonus);
        }

        private class SteadyAimComponent : ChargeableComponent
        {
            private readonly Bow _bow;
            private readonly float _precisionBonus;
            private bool _modify;

            public SteadyAimComponent(IHumanoid Entity, float ChargeTime, float PrecisionBonus) : base(Entity,
                ChargeTime)
            {
                _precisionBonus = PrecisionBonus;
                _bow = (Bow)Entity.LeftWeapon;
                _bow.BowModifiers += AddModifiers;
                Parent.AfterAttack += AfterAttack;
            }

            protected override bool ShouldCharge()
            {
                return Parent.IsMoving;
            }

            protected override void Apply(AttackOptions Options)
            {
                _modify = true;
            }

            private void AddModifiers(Projectile Proj)
            {
                if (!_modify) return;
                Proj.Mesh.Outline = true;
                Proj.Mesh.OutlineColor = Colors.Violet;
                Proj.Mesh.Scale *= 1.15f;
                Proj.Speed *= 1.25f;
                Proj.Falloff = _precisionBonus;
            }

            private void AfterAttack(AttackOptions Options)
            {
                _modify = false;
            }

            public override void Dispose()
            {
                base.Dispose();
                _bow.BowModifiers -= AddModifiers;
                Parent.AfterAttack -= AfterAttack;
            }
        }
    }
}