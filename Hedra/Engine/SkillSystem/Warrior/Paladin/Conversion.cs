using System.Numerics;
using Hedra.AISystem;
using Hedra.Components;
using Hedra.Engine.Rendering.Animation;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Numerics;
using Hedra.Rendering;
using SixLabors.ImageSharp;

namespace Hedra.Engine.SkillSystem.Warrior.Paladin
{
    public class Conversion : WeaponBonusWithAnimationSkill
    {
        private bool _hasMinion;
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Salvation.png");

        protected override Animation SkillAnimation { get; } =
            AnimationLoader.LoadAnimation("Assets/Chr/WarriorImbueAttack.dae");

        protected override float AnimationSpeed => 1.25f;

        public override float IsAffectingModifier => _hasMinion ? 1 : 0 + base.IsAffectingModifier;
        protected override Vector4 OutlineColor => Color.Gold.AsVector4();
        protected override int MaxLevel => 15;
        public override float MaxCooldown => 34;
        public override float ManaCost => 54;
        protected override bool ShouldDisable => _hasMinion;
        private float ConvertChance => .2f + .55f * (Level / (float)MaxLevel);
        public override string Description => Translations.Get("conversion_desc");
        public override string DisplayName => Translations.Get("conversion_skill");

        public override string[] Attributes => new[]
        {
            Translations.Get("conversion_convert_change", (int)(ConvertChance * 100)),
            Translations.Get("conversion_affect_change")
        };

        protected override void ApplyBonusToEnemy(IEntity Victim, ref float Damage)
        {
            if (!Victim.IsBoss() && !Victim.IsHumanoid && Utils.Rng.NextFloat() < ConvertChance)
            {
                _hasMinion = true;
                Victim.RemoveComponent(Victim.SearchComponent<BasicAIComponent>());
                Victim.AddComponent(new MinionAIComponent(Victim, User));
                Victim.Model.Outline = true;
                Victim.Model.OutlineColor = OutlineColor;
                Victim.IsFriendly = true;
                Victim.RemoveComponent(Victim.SearchComponent<HealthBarComponent>());
                Victim.AddComponent(new HealthBarComponent(Victim, Victim.Name, HealthBarType.Gold,
                    OutlineColor.ToColor()));
                Victim.SearchComponent<DamageComponent>()
                    .Ignore(E => E == User || E.SearchComponent<MinionAIComponent>()?.Owner == User);
                Victim.SearchComponent<DamageComponent>().OnDeadEvent += A => _hasMinion = false;
            }
        }
    }
}