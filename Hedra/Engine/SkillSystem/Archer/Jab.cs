using System;
using System.Globalization;
using Hedra.Components.Effects;
using Hedra.Core;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.SkillSystem.Archer
{
    public class Jab : SingleAnimationSkill<IPlayer>
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Jab.png");
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/ArcherJab.dae");
        protected override int MaxLevel => 15;
        public override float MaxCooldown => 8;
        public override float ManaCost => 0;
        protected override bool CanMoveWhileCasting => false;
        protected override float AnimationSpeed => 1.5f;

        protected override void OnExecution()
        {
            User.Movement.Orientate();
        }

        protected override void OnAnimationMid()
        {
            SkillUtils.DamageNearby(User, Damage, 24, .65f);
            SkillUtils.DoNearby(User, 24, .75f, (E, D) =>
            {
                if(Utils.Rng.NextFloat() < BleedChance && E.SearchComponent<BleedingComponent>() == null)
                    E.AddComponent(new BleedingComponent(E, User, 4, Damage * .5f));
            });
        }

        private float Damage => 25 + Level * 2;
        private float BleedChance => Math.Min(.1f + Level / 10f, .75f);
        public override string Description => Translations.Get("jab_desc");
        public override string DisplayName => Translations.Get("jab_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("jab_bleed_change", (int) (BleedChance * 100)),
            Translations.Get("jab_damage_change", Damage.ToString("0.0", CultureInfo.InvariantCulture))
        };
    }
}