using System.Drawing;
using Hedra.Core;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.EntitySystem;
using Hedra.Localization;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Warrior.Paladin
{
    public class HolyAttack : WeaponBonusWithAnimationSkill
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/HolyAttack.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/WarriorImbueAttack.dae");
        protected override float AnimationSpeed => 1.25f;

        protected override void ApplyBonusToEnemy(IEntity Victim, ref float Damage)
        {
            Damage *= 1 + DamageMultiplier;
        }
        
        protected override Vector4 OutlineColor => Color.Gold.ToVector4();
        protected override int MaxLevel => 15;
        public override float ManaCost => 65;
        public override float MaxCooldown => 22;
        private float DamageMultiplier => .75f + 2.5f * (Level / (float) MaxLevel);
        public override string Description => Translations.Get("holy_attack_desc");
        public override string DisplayName => Translations.Get("holy_attack_skill");
        public override string[] Attributes => new []
        {
            Translations.Get("holy_attack_damage_bonus_change", (int)(DamageMultiplier * 100))
        };
    }
}