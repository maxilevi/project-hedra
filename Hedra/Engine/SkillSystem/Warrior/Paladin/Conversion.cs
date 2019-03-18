using System.Drawing;
using Hedra.AISystem;
using Hedra.AISystem.Humanoid;
using Hedra.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Warrior.Paladin
{
    public class Conversion : WeaponBonusWithAnimationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Salvation.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/WarriorImbueAttack.dae");
        private bool _hasMinion;
        
        protected override void ApplyBonusToEnemy(IEntity Victim, ref float Damage)
        {
            if (!Victim.IsBoss && !Victim.IsHumanoid && Utils.Rng.NextFloat() < ConvertChance)
            {
                _hasMinion = true;
                Victim.RemoveComponent(Victim.SearchComponent<BasicAIComponent>());
                Victim.AddComponent(new MinionAIComponent(Victim, Player));
                Victim.Model.Outline = true;
                Victim.Model.OutlineColor = OutlineColor;
                Victim.IsFriendly = true;
                Victim.RemoveComponent(Victim.SearchComponent<HealthBarComponent>());
                Victim.AddComponent(new HealthBarComponent(Victim, Victim.Name, HealthBarType.Gold, OutlineColor.ToColor()));
                Victim.SearchComponent<DamageComponent>().Ignore(E => E == Player || E.SearchComponent<MinionAIComponent>()?.Owner == Player);
                Victim.SearchComponent<DamageComponent>().OnDeadEvent += A => _hasMinion = false;
            }
        }

        public override float IsAffectingModifier => _hasMinion ? 1 : 0 + base.IsAffectingModifier;
        protected override Vector4 OutlineColor => Color.Gold.ToVector4();
        protected override int MaxLevel => 15;
        public override float MaxCooldown => 34;
        public override float ManaCost => 54;
        protected override bool ShouldDisable => _hasMinion;
        private float ConvertChance => .2f + .55f * (Level / (float) MaxLevel);
        public override string Description => Translations.Get("conversion_desc");
        public override string DisplayName => Translations.Get("conversion_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("conversion_convert_change", (int)(ConvertChance * 100)),
            Translations.Get("conversion_affect_change")
        };
    }
}