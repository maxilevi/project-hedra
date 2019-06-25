using System.Globalization;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Localization;
using Hedra.Rendering;

namespace Hedra.Engine.SkillSystem.Rogue.Ninja
{
    public class SnakeSpirit : ActivateDurationSkill<IPlayer>
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/SnakeSpirit.png");
        
        protected override void DoEnable()
        {
            User.Model.Outline = true;
            User.Model.OutlineColor = Colors.GreenYellow;
            User.SearchComponent<DamageComponent>().MissChance = DodgeChance;
        }

        protected override void DoDisable()
        {
            User.Model.Outline = false;
            User.SearchComponent<DamageComponent>().MissChance = DamageComponent.DefaultMissChance;
        }

        private float DodgeChance => .25f + .6f * (Level / (float) MaxLevel);
        protected override float Duration => 4.5f + 4f * (Level / (float) MaxLevel);
        protected override int MaxLevel => 15;
        public override float ManaCost => 60;
        protected override float CooldownDuration => 28;
        public override string Description => Translations.Get("snake_spirit_desc");
        public override string DisplayName => Translations.Get("snake_spirit_skill");
        public override string[] Attributes => new []
        {
            Translations.Get("snake_spirit_duration_change", Duration.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("snake_spirit_dodge_change", (int) (DodgeChance * 100))
        };
    }
}