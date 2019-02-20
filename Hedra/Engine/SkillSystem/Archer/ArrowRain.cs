using System;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.WeaponSystem;

namespace Hedra.Engine.SkillSystem.Archer
{
    public class ArrowRain : SingleAnimationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/ArrowRain.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/ArcherArrowRain.dae");
        protected override int MaxLevel => 10;
        public override float ManaCost => 40f;
        public override float MaxCooldown => 20 - Level;
        
        protected override void OnExecution()
        {
        }

        protected override void OnAnimationMid()
        {
            var weapon = (RangedWeapon)Player.LeftWeapon;
            weapon.Shoot(Player.Orientation, AttackOptions.Default);
            weapon.Shoot(Player.Orientation, AttackOptions.Default);
            weapon.Shoot(Player.Orientation, AttackOptions.Default);
        }
        
        public override bool MeetsRequirements()
        {
            return base.MeetsRequirements() && !Player.Toolbar.DisableAttack && Player.HasWeapon && Player.LeftWeapon is RangedWeapon;
        }

        public override string Description => Translations.Get("arrow_rain_desc");
        public override string DisplayName => Translations.Get("arrow_rain");
        protected override bool Grayscale => !Player.HasWeapon;
    }
}